namespace Paradiso.API.Domain.Models.Auxiliars;

public class AuxiliarPostParams
{
    public class LogVisualizationPostParams
    {
        /// <summary>
        /// Id do User. O string guid do perfil visto
        /// </summary>
        public Guid IdUser { get; set; }

        /// <summary>
        /// Id do Viewer. O string guid de quem viu o perfil
        /// </summary>
        public Guid IdViewer { get; set; }
    }
}
