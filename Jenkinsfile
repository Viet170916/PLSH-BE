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

    
         
        stage('SonarQube Scan') {
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

                        sleep 30
                        def timestamp = new Date().format("yyyyMMdd_HHmmss")
                        env.TIMESTAMP = timestamp

                        // Tải issues và sinh HTML báo cáo
                        sh """
                            curl -u $SONAR_TOKEN: "$SONAR_SERVER/api/issues/search?componentKeys=plsh-be&impactSeverities=BLOCKER,HIGH,MEDIUM&statuses=OPEN,CONFIRMED" \
                            -o issues_${timestamp}.json
                        """

                        sh "python3 convert_issue_json.py issues_${timestamp}.json sonarqube-report-${timestamp}.html"
                        archiveArtifacts artifacts: "sonarqube-report-${timestamp}.html", fingerprint: true

                        // Kiểm tra BLOCKER và gửi Telegram nếu có
                        def blockerIssues = []
                        def sonarIssuesJson = readJSON file: "issues_${timestamp}.json"

                        sonarIssuesJson.issues.each { issue ->
                            if (issue.severity == "BLOCKER") {
                                blockerIssues.add(issue)
                            }
                        }

                        if (blockerIssues.size() > 0) {
                            echo "❌ Phát hiện ${blockerIssues.size()} lỗi BLOCKER trong SonarQube!"

                            def msg = URLEncoder.encode("🚨 CI Failed 🚨\\nDự án: PLSH-BE\\nBLOCKER issues: ${blockerIssues.size()}\\nXem chi tiết trong file đính kèm.", "UTF-8")
                            def bot_token = "8104427238:AAGKMJERkz8Z0nZbNJRFoIhw0CKzVgakBGk"
                            def chat_id = "-1002608374616"

                            // Gửi mess
                            sh """
                                curl -s -X POST https://api.telegram.org/bot${bot_token}/sendMessage \\
                                -d chat_id=${chat_id} \\
                                -d text="${msg}"
                            """

                            // Gửi report HTML
                            sh """
                                curl -s -X POST https://api.telegram.org/bot${bot_token}/sendDocument \\
                                -F chat_id=${chat_id} \\
                                -F document=@sonarqube-report-${timestamp}.html
                            """

                            // Dừng pipeline
                            error("⛔️ Dừng pipeline vì có BLOCKER issues trong SonarQube.")
                        }
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
        }


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


        stage('Push BE Image to Docker Hub') {
            steps {
                script {
                    sh '''
                        # Đăng nhập Docker Hub
                        echo $DOCKERHUB_CREDENTIALS_PSW | docker login -u $DOCKERHUB_CREDENTIALS_USR --password-stdin

                        # Tag và push image backend
                        docker tag plsh-be co0bridae/plsh-be:latest
                        docker push co0bridae/plsh-be:latest
                    '''
                }
            }
        }

        stage('Deploy BE to Staging') {
            steps {
                script {
                    def deploying = """
                        #!/bin/bash

                        docker ps -q --filter "name=plsh-be" && docker stop plsh-be || true
                        docker ps -a -q --filter "name=plsh-be" && docker rm plsh-be || true

                        docker pull co0bridae/plsh-be:latest

                        docker run -d \\
                        --name plsh-be \\
                        -p 5000:5000 \\
                        -e ASPNETCORE_ENVIRONMENT=Staging \\
                        co0bridae/plsh-be:latest
                    """

                    sshagent(['jenkins-ssh-key']) {
                        sh """
                            ssh -o StrictHostKeyChecking=no root@192.168.230.101 'echo "${deploying}" > /root/deploy-be.sh && chmod +x /root/deploy-be.sh && /root/deploy-be.sh'
                        """
                    }
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