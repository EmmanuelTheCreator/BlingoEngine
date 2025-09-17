using BlingoEngine.Stages;
using UnityEngine;

namespace BlingoEngine.Unity.Stages;

/// <summary>
/// Unity implementation of <see cref="IBlingoFrameworkStageContainer"/>.
/// Hosts the stage inside a dedicated GameObject.
/// </summary>
public class UnityStageContainer : IBlingoFrameworkStageContainer
{
    private readonly GameObject _container;
    private UnityStage? _stage;

    public UnityStageContainer()
    {
        _container = new GameObject("StageContainer");
    }

    /// <summary>Root GameObject containing the stage.</summary>
    public GameObject Container => _container;

    public void SetStage(IBlingoFrameworkStage stage)
    {
        _stage = stage as UnityStage;
        if (_stage != null)
            _stage.transform.parent = _container.transform;
    }

    /// <summary>Adjusts the scale of the current stage.</summary>
    public void SetScale(float scale)
    {
        if (_stage != null)
        {
            _stage.Scale = scale;
        }
    }
}

