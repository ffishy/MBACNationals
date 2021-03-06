﻿using Edument.CQRS;
using Events.Participant;
using MBACNationals.Participant;
using MBACNationals.Participant.Commands;
using MBACNationals.ReadModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace MBACNationalsTests
{
    [TestClass]
    public class ParticipantTests : BDDTest<ParticipantCommandHandlers, ParticipantAggregate>
    {
        private Guid alternateId;
        private Guid participantId;
        private Guid teamId;
        private string name;
        private string newName;

        [TestInitialize]
        public void Setup()
        {
            alternateId = Guid.NewGuid();
            participantId = Guid.NewGuid();
            teamId = Guid.NewGuid();
            name = "John";
            newName = "David";

            var alternateMock = new Mock<CommandQueries.Participant>();
            alternateMock.Object.Id = alternateId;
            alternateMock.SetupGet(x => x.Name).Returns("alternate");

            var commandQueriesMock = new Mock<ICommandQueries>();
            commandQueriesMock
                    .Setup(m => m.GetParticipant(alternateId))
                    .Returns(alternateMock.Object);
            SystemUnderTest(new ParticipantCommandHandlers(commandQueriesMock.Object));
        }

        [TestMethod]
        public void CanCreateParticipant()
        {
            Test(
                Given(),
                When(new CreateParticipant
                {
                    Id = participantId,
                    Name = name,
                    Gender = "M",
                    IsDelegate = true,
                    YearsQualifying = 10,
                }),
                Then(
                    new ParticipantCreated
                    {
                        Id = participantId,
                        Name = name,
                        Gender = "M",
                        IsDelegate = true,
                        YearsQualifying = 10,
                    },
                    new ParticipantAverageChanged
                    {
                        Id = participantId,
                    },
                    new ParticipantShirtSizeChanged
                    {
                        Id = participantId,
                    })
                );
        }

        [TestMethod]
        public void CanNotDuplicateParticipant()
        {
            Test(
                Given(new ParticipantCreated
                {
                    Id = participantId,
                    Name = name
                }),
                When(new CreateParticipant
                {
                    Id = participantId,
                    Name = name
                }),
                ThenFailWith<ParticipantAlreadyExists>());
        }

        [TestMethod]
        public void CanRenameParticipant()
        {
            Test(
                Given(new ParticipantCreated
                {
                    Id = participantId,
                    Name = name
                }),
                When(new RenameParticipant
                {
                    Id = participantId,
                    Name = newName
                }),
                Then(new ParticipantRenamed
                {
                    Id = participantId,
                    Name = newName
                }));
        }

        [TestMethod]
        public void CanAssignParticipantToTeam()
        {
            Test(
                Given(),
                When(new AddParticipantToTeam
                {
                    Id = participantId,
                    TeamId = teamId
                }),
                Then(new ParticipantAssignedToTeam
                {
                    Id = participantId,
                    TeamId = teamId
                }));
        }

        [TestMethod]
        public void CanAssignParticipantToDifferentTeam()
        {
            Test(
                Given(new ParticipantAssignedToTeam
                {
                    Id = participantId,
                    TeamId = Guid.NewGuid(),
                }),
                When(new AddParticipantToTeam
                {
                    Id = participantId,
                    TeamId = teamId
                }),
                Then(new ParticipantAssignedToTeam
                {
                    Id = participantId,
                    TeamId = teamId
                }));
        }

        [TestMethod]
        public void AssignParticipantToSameTeamDoesNothing()
        {
            Test(
                Given(new ParticipantAssignedToTeam
                {
                    Id = participantId,
                    TeamId = teamId,
                }),
                When(new AddParticipantToTeam
                {
                    Id = participantId,
                    TeamId = teamId
                }),
                Then());
        }
    }
}
