pipeline {
    agent { label 'agent_server' }

    environment {
        DOCKERHUB_CREDENTIALS = credentials('dockerhub-credentials')
        SONAR_SERVER = credentials('sonarqube-server-url')
        STAGING_SERVER = credentials('staging-server-url')
        SNYK_TOKEN = credentials('snyk-api-token')
        SONAR_TOKEN = credentials('g67_se490_spr25')
    }

    stages {
        stage('Info') {
            steps {
                sh(script: """ 
                    whoami
                    pwd
                    ls -la 
                    dotnet --version
                """, label: "Initial info")
            }
        }

    
         
    /*    stage('SonarQube Scan') {
            steps {
                script {
                    dir('PLSH-BE') {
                        withSonarQubeEnv('Sonarqube server connection') {
                    sh '''
                        export PATH="$PATH:$HOME/.dotnet/tools"

                        # Bắt đầu phân tích SonarQube
                        dotnet sonarscanner begin \
                            /k:"plsh-be" \
                            /d:sonar.host.url=$SONAR_HOST_URL \
                            /d:sonar.login=$SONAR_AUTH_TOKEN

                        # Build solution
                        dotnet build PLSH-BE.sln

                        # Kết thúc phân tích
                        dotnet sonarscanner end \
                            /d:sonar.login=$SONAR_AUTH_TOKEN
                    '''
                }


                        // Delay một chút để SonarQube xử lý kết quả
                        sleep 30

                        // Bước 4: Tải issues và sinh báo cáo HTML
                        def timestamp = new Date().format("yyyyMMdd_HHmmss")
                        env.TIMESTAMP = timestamp

                        sh """
                            curl -u $SONAR_TOKEN: "$SONAR_SERVER/api/issues/search?componentKeys=plsh-be&impactSeverities=BLOCKER,HIGH,MEDIUM&statuses=OPEN,CONFIRMED" \
                            -o issues_${timestamp}.json
                        """

                        sh "python3 convert_issue_json.py issues_${timestamp}.json sonarqube-report-${timestamp}.html"

                        archiveArtifacts artifacts: "sonarqube-report-${timestamp}.html", fingerprint: true
                    }
                }
            }
        }


        stage('Snyk Scan') {
            steps {
                dir('PLSH-BE') {
                    script {
                        // Set Snyk Token
                        sh 'snyk config set api=$SNYK_TOKEN'

                        def timestamp = new Date().format("yyyyMMdd_HHmmss")
                        env.TIMESTAMP = timestamp

                        // Snyk test và sinh báo cáo
                        sh """
                            snyk test --file=PLSH-BE.sln --severity-threshold=high --json-file-output=snyk.json || true
                            [ -f snyk.json ] && snyk-to-html -i snyk.json -o snyk-report-${timestamp}.html || true
                        """

                        archiveArtifacts artifacts: "snyk-report-${timestamp}.html", fingerprint: true
                    }
                }
            }
        }*/


        stage('Build Docker Image') {
            steps {
                script {
                    dir('PLSH-BE') {
                        sh '''
                            docker build -t plsh-be -f Dockerfile .
                            docker tag plsh-be co0bridae/plsh-be:latest
                        '''
                    }
                }
            }
        }

        stage('Trivy Scan') {
            steps {
                script {
                    def timestamp = new Date().format("yyyyMMdd_HHmmss")
                    env.TIMESTAMP = timestamp

                    sh """
                        # Scan Docker image với Trivy
                        trivy image --timeout 10m --format json --output plsh-be-trivy-${timestamp}.json --severity HIGH,CRITICAL plsh-be:latest

                        # Convert kết quả JSON thành HTML
                        python3 convert_json.py plsh-be-trivy-${timestamp}.json plsh-be-trivy-${timestamp}.html
                    """

                    archiveArtifacts artifacts: "plsh-be-trivy-${timestamp}.html", fingerprint: true
                }
            }
        }


        stage('Push to Docker Hub') {
            steps {
                script {
                    sh '''
                        echo $DOCKERHUB_CREDENTIALS_PSW | docker login -u $DOCKERHUB_CREDENTIALS_USR --password-stdin
                        docker push co0bridae/plsh-be:latest
                    '''
                }
            }
        }

        stage('Deploy to Staging') {
            steps {
                script {
                    def deployScript = """
                        #!/bin/bash
                        echo "Stopping existing container..."
                        docker ps -q --filter "name=plsh-be" | xargs -r docker stop
                        docker ps -a -q --filter "name=plsh-be" | xargs -r docker rm

                        echo "Pulling latest image..."
                        docker pull co0bridae/plsh-be:latest

                        echo "Starting new container..."
                        docker run -d --name plsh-be -p 5000:5000 -e ASPNETCORE_ENVIRONMENT=Staging co0bridae/plsh-be:latest
                    """

                    sshagent(['jenkins-ssh-key']) {
                        sh """
                            ssh -o StrictHostKeyChecking=no ${env.STAGING_SERVER} 'echo "${deployScript}" > /tmp/deploy_plsh.sh && chmod +x /tmp/deploy_plsh.sh && /tmp/deploy_plsh.sh'
                        """
                    }
                }
            }
        }

        stage('ZAP Scan') {
            steps {
                script {
                    sh """
                        cd /opt/zaproxy
                        ./zap.sh -daemon -port 8090 -host 0.0.0.0 -config api.disablekey=true -config api.addrs.addr.name=.* &

                        READY=0
                        while [ \$READY -eq 0 ]; do
                            if curl -s "http://localhost:8090/JSON/core/view/version/" | grep "version"; then
                                READY=1
                            else
                                echo "Waiting for ZAP to start..."
                                sleep 5
                            fi
                        done

                        curl -s "http://localhost:8090/JSON/spider/action/scan/?url=${env.STAGING_SERVER}&contextName=PLSH&recurse=true"
                        sleep 30

                        curl -s "http://localhost:8090/JSON/ascan/action/scan/?url=${env.STAGING_SERVER}&contextName=PLSH"
                        sleep 120

                        curl -s "http://localhost:8090/OTHER/core/other/htmlreport/" -o "${env.PROJECT_PATH}/zap_report.html"
                        curl "http://localhost:8090/JSON/core/action/shutdown/"
                    """
                    archiveArtifacts artifacts: 'zap_report.html', fingerprint: true
                }
            }
        }
    }

    post {
        always {
            cleanWs()
        }
        success {
            slackSend(color: "good", message: "PLSH-BE Pipeline SUCCESSFUL: ${env.JOB_NAME} ${env.BUILD_NUMBER}")
        }
        failure {
            slackSend(color: "danger", message: "PLSH-BE Pipeline FAILED: ${env.JOB_NAME} ${env.BUILD_NUMBER}")
        }
    }
}