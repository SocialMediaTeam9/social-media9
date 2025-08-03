resource "aws_sqs_queue" "inbound_queue" {
  name = "${var.project_name}-inbound-queue"
}

resource "aws_sqs_queue" "outbound_queue" {
  name = "${var.project_name}-outbound-queue"
}