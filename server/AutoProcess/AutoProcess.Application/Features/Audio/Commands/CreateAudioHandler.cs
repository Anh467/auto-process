// Application/Features/Audio/Commands/CreateAudio/CreateAudioHandler.cs
using AutoProcess.Application.Common.Interfaces.Repositories;
using AutoProcess.Application.Features.CQRS;
using AutoProcess.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AutoProcess.Application.Features.Audio.Commands
{
    public class CreateAudioHandler : IComandHandler<CreateAudioCommand>
    {
        private readonly IAudioRepository _repository;

        public CreateAudioHandler(IAudioRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(CreateAudioCommand command, CancellationToken cancellationToken)
        {
            var audio = new Domain.Entities.Audio
            {
                Id = Guid.NewGuid(),
                Name = command.Body.Name,
                Path = command.Body.Path,
                Url = command.Body.Url ?? string.Empty
            };
            await _repository.SaveAsync(audio);
        }
    }

    public class CreateAudioCommand : IComand
    {
        public BodyParams Body { get; }

        public CreateAudioCommand(BodyParams body)
        {
            Body = body;
        }

        public class BodyParams
        {
            [Required(ErrorMessage = "Name is required")]
            [MaxLength(255)]
            public required string Name { get; set; }

            [Required(ErrorMessage = "Path is required")]
            [MaxLength(500)]
            public required string Path { get; set; }

            [MaxLength(500)]
            public string? Url { get; set; }
        }
    }
}