pipeline {
    agent { label 'agent_server' }

    environment {
        DOCKERHUB_CREDENTIALS = credentials('dockerhub-credentials')
        SONAR_SERVER = credentials('sonarqube-server-url')
        STAGING_SERVER = credentials('staging-server-url')
        SNYK_TOKEN = credentials('snyk-api-token')
        GITLAB_TOKEN = credentials('g4_se1818net_token')
        PROJECT_PATH = '/var/lib/jenkins/workspace/Lab_iap491/G76_SEP490_SPR25_/PLSH-BE/PLSH-BE'
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

        stage('Restore & Build') {
            steps {
                dir(env.PROJECT_PATH) {
                    sh 'dotnet restore PLSH-BE.sln'
                    sh 'dotnet build PLSH-BE.sln --configuration Release --no-restore'
                }
            }
        }

         
        stage('Install dotnet-sonarscanner') {
    steps {
        sh 'dotnet tool install --global dotnet-sonarscanner'
        script {
            env.PATH = "${env.HOME}/.dotnet/tools:${env.PATH}"
        }
    }
}


stage('SonarQube Scan') {
    steps {
        dir(env.PROJECT_PATH) {
            withSonarQubeEnv('Sonarqube server connection') {
                sh '''
                    dotnet sonarscanner begin /k:"plsh-be" \
                        /d:sonar.host.url=$SONAR_SERVER \
                        /d:sonar.login=$GITLAB_TOKEN \
                        /d:sonar.exclusions="**/bin/**/*,**/obj/**/*,**/Test*/**/*"

                    dotnet build PLSH-BE.sln --configuration Release

                    dotnet sonarscanner end /d:sonar.login=$GITLAB_TOKEN
                '''
            }
        }
    }
}


        stage('Snyk Scan') {
            steps {
                script {
                    dir(env.PROJECT_PATH) {
                        sh 'snyk config set api=${SNYK_TOKEN}'
                        sh '''
                            dotnet restore PLSH-BE.sln
                            snyk test --severity-threshold=high --json-file-output=snyk-report.json . || true
                            snyk-to-html -i snyk-report.json -o snyk-report.html || true
                        '''
                        archiveArtifacts artifacts: 'snyk-report.html', fingerprint: true
                    }
                }
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    dir(env.PROJECT_PATH) {
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
                    dir(env.PROJECT_PATH) {
                        sh '''
                            trivy image --timeout 10m --format json --output trivy-report.json --severity HIGH,CRITICAL plsh-be:latest
                            python3 convert_json.py trivy-report.json trivy-report.html
                        '''
                        archiveArtifacts artifacts: 'trivy-report.html', fingerprint: true
                    }
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