resource "aws_sqs_queue" "main" {
  name = "${var.project_name}-main-queue"
}