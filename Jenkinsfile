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

        stage('Checkout') {
            steps {
                checkout scm
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
                            echo "SonarQube phát hiện ${blockerIssues.size()} lỗi BLOCKER!"

                            def msg = URLEncoder.encode("⚠️ Pipeline Lab_iap491/G76_SEP490_SPR25_/PLSH-BE Failed. SonarQube đã phát hiện ${blockerIssues.size()} lỗi BLOCKER. Xem chi tiết trong file đính kèm.", "UTF-8")
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
                            error("Dừng pipeline vì SonarQube phát hiện có BLOCKER issues.")
                        }

                    }
                }
            }
        }*/



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

                        // Đọc file JSON và lọc lỗi nghiêm trọng
                        def snykData = readJSON file: "snyk.json"
                        def criticalIssues = 0
                        def highIssues = 0

                        snykData.each { project ->
                            if (project.vulnerabilities) {
                                project.vulnerabilities.each { vuln ->
                                    if (vuln.severity == "critical") {
                                        criticalIssues++
                                    } else if (vuln.severity == "high") {
                                        highIssues++
                                    }
                                }
                            }
                        }

                        // Nếu có lỗi nghiêm trọng → gửi Telegram + fail pipeline
                        if (criticalIssues > 0 || highIssues > 0) {
                            echo "Snyk phát hiện ${criticalIssues} lỗi CRITICAL và ${highIssues} lỗi HIGH!"

                            def msg = URLEncoder.encode("⚠️ Pipeline Lab_iap491/G76_SEP490_SPR25_/PLSH-BE Failed. Snyk phát hiện ${criticalIssues} lỗi CRITICAL và ${highIssues} lỗi HIGH. Xem chi tiết trong file đính kèm.", "UTF-8")
                            def bot_token = "8104427238:AAGKMJERkz8Z0nZbNJRFoIhw0CKzVgakBGk"
                            def chat_id = "-1002608374616"

                            // Gửi message
                            sh """
                                curl -s -X POST https://api.telegram.org/bot${bot_token}/sendMessage \\
                                -d chat_id=${chat_id} \\
                                -d text="${msg}"
                            """

                            // Gửi report HTML
                            sh """
                                curl -s -X POST https://api.telegram.org/bot${bot_token}/sendDocument \\
                                -F chat_id=${chat_id} \\
                                -F document=@snyk-report-${timestamp}.html
                            """

                            // Dừng pipeline
                            error("Dừng pipeline vì Snyk phát hiện lỗi nghiêm trọng.")
                        } else {
                            echo "✅ Không có lỗi CRITICAL hoặc HIGH từ Snyk."
                        }
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

                    // Trivy scan & convert HTML
                    sh """
                        trivy image --timeout 10m --format json --output plsh-be-trivy-${timestamp}.json plsh-be:latest

                        python3 convert_json.py plsh-be-trivy-${timestamp}.json plsh-be-trivy-${timestamp}.html
                    """

                    archiveArtifacts artifacts: "plsh-be-trivy-${timestamp}.html", fingerprint: true

                    // Đọc file JSON và lọc lỗi nghiêm trọng
                    def trivyData = readJSON file: "plsh-be-trivy-${timestamp}.json"
                    def criticalCount = 0
                    def highCount = 0

                    trivyData.Results.each { result ->
                        // Phần vulnerabilities (các package lỗi)
                        result.Vulnerabilities?.each { vuln ->
                            if (vuln.Severity == "CRITICAL") {
                                criticalCount++
                            } else if (vuln.Severity == "HIGH") {
                                highCount++
                            }
                        }

                        // Phần secrets (như private key, service account key)
                        result.Secrets?.each { secret ->
                            if (secret.Severity == "CRITICAL") {
                                criticalCount++
                            } else if (secret.Severity == "HIGH") {
                                highCount++
                            }
                        }
                    }

                    if (criticalCount > 0 || highCount > 0) {
                        echo "Trivy phát hiện ${criticalCount} lỗi CRITICAL và ${highCount} lỗi HIGH!"

                        def msg = URLEncoder.encode("⚠️ Pipeline Lab_iap491/G76_SEP490_SPR25_/PLSH-BE Failed. Trivy phát hiện ${criticalCount} lỗi CRITICAL và ${highCount} lỗi HIGH. Xem chi tiết trong báo cáo đính kèm.", "UTF-8")
                        def bot_token = "8104427238:AAGKMJERkz8Z0nZbNJRFoIhw0CKzVgakBGk"
                        def chat_id = "-1002608374616"

                        // Gửi message Telegram
                        sh """
                            curl -s -X POST https://api.telegram.org/bot${bot_token}/sendMessage \\
                            -d chat_id=${chat_id} \\
                            -d text="${msg}"
                        """

                        // Gửi file báo cáo HTML
                        sh """
                            curl -s -X POST https://api.telegram.org/bot${bot_token}/sendDocument \\
                            -F chat_id=${chat_id} \\
                            -F document=@plsh-be-trivy-${timestamp}.html
                        """

                        error("Dừng pipeline vì Trivy phát hiện lỗi nghiêm trọng.")
                    } else {
                        echo "Trivy không phát hiện lỗi CRITICAL hoặc HIGH."
                    }
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



        


        
    }
}