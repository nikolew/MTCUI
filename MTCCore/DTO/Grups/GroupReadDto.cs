using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using Windows.UI;



namespace MTCCore.DTO.Grups
{
    public record GroupReadDto(int Id, string Name, IReadOnlyList<string> Times, Color color) { }
}
