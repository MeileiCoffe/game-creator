using GameCreator.Api.Engine;
using static GameCreator.Api.Engine.GameMakerVersion;

namespace GameCreator.Engine.Library;

public interface ITimingFunctions
{
    #region Deprecated Functions
    #endregion
    
    [Gml("sleep", v11)]
    void Sleep(double numb);

}
