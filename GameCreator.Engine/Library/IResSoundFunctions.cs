using GameCreator.Api.Engine;
using static GameCreator.Api.Engine.GameMakerVersion;

namespace GameCreator.Engine.Library;

public interface IResSoundFunctions
{
    #region Deprecated Functions
    #endregion

    //
    // Introduced in v4.0
    //
    [Gml("sound_discard", v40)]
    void SoundDiscard(double index);

    [Gml("sound_restore", v40)]
    void SoundRestore(double index);
}
