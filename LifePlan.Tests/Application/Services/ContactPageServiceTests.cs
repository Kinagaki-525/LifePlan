using LifePlan.Application.Dto;
using LifePlan.Application.Interfaces;
using LifePlan.Application.Services;
using LifePlan.ViewModels.Contact;
using Microsoft.Extensions.Logging.Abstractions;

namespace LifePlan.Tests.Application.Services;

public class ContactPageServiceTests
{
    [Fact]
    public async Task Submit_DoesNotSendEmailWhenBindingErrorsExist()
    {
        // Arrange
        var emailSender = new FakeEmailSender();
        var service = CreateService(emailSender);
        var input = CreateValidInput();

        // Act
        var result = await service.Submit(input, hasBindingErrors: true);

        // Assert
        Assert.False(result.IsSent);
        Assert.Null(result.ErrorMessage);
        Assert.Same(input, result.Page);
        Assert.Equal(0, emailSender.SendCount);
    }

    [Fact]
    public async Task Submit_ReturnsSentResultWhenEmailSendSucceeds()
    {
        // Arrange
        var emailSender = new FakeEmailSender();
        var service = CreateService(emailSender);
        var input = CreateValidInput();

        // Act
        var result = await service.Submit(input, hasBindingErrors: false);

        // Assert
        Assert.True(result.IsSent);
        Assert.Null(result.ErrorMessage);
        Assert.Same(input, result.Page);
        Assert.Equal(1, emailSender.SendCount);
        Assert.NotNull(emailSender.LastMessage);
        Assert.Equal(input.Name, emailSender.LastMessage.Name);
        Assert.Equal(input.Email, emailSender.LastMessage.Email);
        Assert.Equal(input.Subject, emailSender.LastMessage.Subject);
        Assert.Equal(input.Message, emailSender.LastMessage.Message);
    }

    [Fact]
    public async Task Submit_ReturnsErrorResultWhenEmailSendFails()
    {
        // Arrange
        var emailSender = new FakeEmailSender
        {
            ExceptionToThrow = new InvalidOperationException("SMTP failed")
        };
        var service = CreateService(emailSender);
        var input = CreateValidInput();

        // Act
        var result = await service.Submit(input, hasBindingErrors: false);

        // Assert
        Assert.False(result.IsSent);
        Assert.NotNull(result.ErrorMessage);
        Assert.Same(input, result.Page);
        Assert.Equal(1, emailSender.SendCount);
    }

    private static ContactPageService CreateService(IEmailSender emailSender)
    {
        return new ContactPageService(emailSender, NullLogger<ContactPageService>.Instance);
    }

    private static ContactViewModel CreateValidInput()
    {
        return new ContactViewModel
        {
            Name = "Test User",
            Company = "Test Company",
            Email = "test@example.com",
            Category = "service",
            Subject = "Test subject",
            Message = "Test message",
            AgreePrivacy = true
        };
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public int SendCount { get; private set; }

        public ContactEmailMessage? LastMessage { get; private set; }

        public Exception? ExceptionToThrow { get; init; }

        public Task SendContactAsync(ContactEmailMessage message)
        {
            SendCount++;
            LastMessage = message;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.CompletedTask;
        }
    }
}
