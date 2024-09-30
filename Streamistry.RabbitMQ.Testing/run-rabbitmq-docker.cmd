@ECHO OFF
ECHO Starting docker container for RabbitMQ with Management plugin

docker run -d --hostname my-rabbit --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
