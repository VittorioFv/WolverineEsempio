using EFinfrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using WolverineAPI.Handler;
using WolverineAPI.Messages;

namespace Test;

public class UnitTest1
{
    [Fact]
    public async void CreateItemTest_ReturnItemCreated()
    {
        //Arrange
        Mock<DbSet<Item>> mockSet = new Mock<DbSet<Item>>();
        Mock<ItemDbContext> mockContext = new Mock<ItemDbContext>();

        mockContext.Setup(m => m.Items).Returns(mockSet.Object);

        var item = new Item
        {
            Name = "Test",
        };
        var command = new CreateItemCommand(item);

        //Act
        var messages = await CreateItemHandler.Handle(command, mockContext.Object);

        //Assert
        mockSet.Verify(
            m => m.Add(It.IsAny<Item>()),
            Times.Once);

        mockContext.Verify(
            m => m.SaveChanges(),
            Times.Never);

        mockContext.Verify(
            m => m.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.True(
            messages is ItemCreated
            );
    }
}