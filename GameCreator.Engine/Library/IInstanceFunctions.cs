using GameCreator.Api.Engine;
using static GameCreator.Api.Engine.GameMakerVersion;

namespace GameCreator.Engine.Library;

public interface IInstanceFunctions
{
    #region Deprecated Functions
    [Gml("create", v11, v33)]
    void Create(double x, double y, int obj);

    [Gml("change", v11, v33)]
    void Change(int obj1, int obj2);

    [Gml("change_at", v11, v33)]
    void ChangeAt(double x, double y, int obj);

    [Gml("destroy", v11, v33)]
    void Destroy(int obj);

    [Gml("destroy_at", v11, v33)]
    void DestroyAt(double x, double y);
    
    [Gml("number", v11, v33)]
    int Number(int obj);
    #endregion

    //
    // Introduced in v4.0
    //

    [Gml("instance_find", v40)]
    double InstanceFind(string obj, double n);

    [Gml("instance_exists", v40)]
    double InstanceExists(string obj);

    [Gml("instance_number", v40)]
    double InstanceNumber(string obj);

    [Gml("instance_position", v40)]
    double InstancePosition(double x, double y, string obj);

    [Gml("instance_nearest", v40)]
    double InstanceNearest(double x, double y, string obj);

    [Gml("instance_furthest", v40)]
    double InstanceFurthest(double x, double y, string obj);

    [Gml("instance_place", v40)]
    double InstancePlace(double x, double y, string obj);

    [Gml("instance_create", v40)]
    double InstanceCreate(double x, double y, string obj);

    [Gml("instance_destroy", v40)]
    void InstanceDestroy();

    [Gml("instance_change", v40)]
    void InstanceChange(string obj, double perf);

    [Gml("position_destroy", v40)]
    void PositionDestroy(double x, double y);

    [Gml("position_change", v40)]
    void PositionChange(double x, double y, string obj, double perf);
}
