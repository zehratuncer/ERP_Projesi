using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Common.Interfaces;
using ERP.Application.Features.PurchaseRequests.Commands.RejectPurchaseRequest;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.UnitTests.Common;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace ERP.UnitTests.PurchaseRequests;

public class RejectPurchaseRequestTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly RejectPurchaseRequestCommandValidator _validator = new();

    public RejectPurchaseRequestTests()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
    }

    [Fact]
    public async Task RejectPurchaseRequest_WithValidReason_ShouldSetStatusToRejected()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var p = context.Products.First();
        var request = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = "TALEP-REJ-001",
            Department = "Muhasebe",
            Priority = RequestPriority.Low,
            Status = RequestStatus.PendingApproval,
            TotalEstimatedAmount = 500m
        };

        context.PurchaseRequests.Add(request);
        await context.SaveChangesAsync();

        var handler = new RejectPurchaseRequestCommandHandler(context, _currentUserServiceMock.Object, _notificationServiceMock.Object);
        var command = new RejectPurchaseRequestCommand(request.Id, "Bütçe aşımı sebebiyle onaylanmadı.");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(RequestStatus.Rejected);

        var history = context.ApprovalHistories.FirstOrDefault(h => h.PurchaseRequestId == request.Id);
        history.Should().NotBeNull();
        history!.Action.Should().Be(ApprovalAction.Rejected);
        history.Comment.Should().Be("Bütçe aşımı sebebiyle onaylanmadı.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("123")]
    public void RejectPurchaseRequestValidator_ShortOrEmptyReason_ShouldFailValidation(string invalidReason)
    {
        var command = new RejectPurchaseRequestCommand(Guid.NewGuid(), invalidReason);
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
    }
}
