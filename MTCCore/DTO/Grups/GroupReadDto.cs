using System.Collections.Generic;

namespace MTCCore.DTO.Grups
{
    public record GroupReadDto(int Id, string Name, IReadOnlyList<string> Times);
}
