using Paradiso.API.Domain.Dtos;
using Paradiso.API.Domain.Entities;
using static Paradiso.API.Domain.Models.Auxiliars.AuxiliarGetParams;
using static Paradiso.API.Domain.Models.Auxiliars.AuxiliarPostParams;

namespace Paradiso.API.Domain.Interfaces;

public interface IAuxiliarHandler
{
    Task<MessageDto> UploadLogVisualizationAsync(LogVisualizationPostParams @params);

    Task<List<City>> GetCityAsync(CityGetParams @params);

    Task<List<State>> GetStateAsync(StateGetParams @params);

    Task<List<Area>> GetAreaAsync(AreaGetParams @params);

    Task<List<Genre>> GetGenreAsync(GenreGetParams @params);

    Task<List<KindMovie>> GetKindMovieAsync(KindMovieGetParams @params);
}
