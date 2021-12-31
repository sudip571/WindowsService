# Creating  Windows Service

This porject has been developed as an example/guidance for writing windows service.It includes pretty much everything e.g. Logging,sending Email.
Some of the  libraries  used on this project are
* Topshelf 
* Serilog
* Dapper
* MailKit
* FluentScheduler

#### About the Project

In short ,this windows service monitors other windows services and if those services are stucked for long, it  tries to  restart.It also keeps logging and send email about the status.
