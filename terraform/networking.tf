data "aws_acm_certificate" "af_cert" {
  domain   = var.domain_name
  statuses = ["ISSUED"]
  most_recent = true
}

resource "aws_lb" "main" {
  name               = "${var.project_name}-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb_sg.id]
  subnets            = module.vpc.public_subnets
}

resource "aws_lb_target_group" "web_api" {
  name        = "${var.project_name}-web-api-tg"
  port        = var.app_container_port
  protocol    = "HTTP"
  vpc_id      = module.vpc.vpc_id
  target_type = "ip"

  health_check {
    path                = "/health"
    protocol            = "HTTP"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }
}


# Federation
resource "aws_lb_target_group" "gts_sidecar" {
  name        = "${var.project_name}-gts-sidecar-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = module.vpc.vpc_id
  target_type = "ip"


  health_check { path = "/api/v1/instance" }
}

resource "aws_lb_listener" "main_listener" {
  load_balancer_arn = aws_lb.main.arn
  port              = 443
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-2016-08"
  certificate_arn   = data.aws_acm_certificate.af_cert.arn

  default_action {
    type = "fixed-response"
    fixed_response {
      content_type = "text/plain"
      message_body = "Cannot route request"
      status_code  = "404"
    }
  }
}

resource "aws_lb_listener" "http_redirect" {
  load_balancer_arn = aws_lb.main.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type = "redirect"
    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
    }
  }
}

resource "aws_lb_listener_rule" "csharp_api_rule" {
  listener_arn = aws_lb_listener.main_listener.arn
  priority     = 10

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.web_api.arn
  }

  condition {
    host_header {
      values = ["api.${var.domain}"]
    }
  }
}

resource "aws_lb_listener_rule" "federation_host_rule" {
  listener_arn = aws_lb_listener.main_listener.arn
  priority     = 20

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.gts_sidecar.arn
  }

  condition {
    host_header {
      values = ["fed.${var.domain}"]
    }
  }
}

resource "aws_lb_listener" "internal_api_listener" {
  load_balancer_arn = aws_lb.main.arn
  port              = var.internal_api_port
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.web_api.arn
  }
}


