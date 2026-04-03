using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ayla;

public class UIManager : Singleton<UIManager>
{
    private Transform m_UIRoot = null!;
    private readonly SortedList<int, UILayer> m_Layers = new();
    private Action<UILayer, Canvas>? m_Configure;

    protected override void Awake()
    {
        base.Awake();
        m_UIRoot = this.MakeChild("UIManager");

        AddLayer("Overlay", UILayer.kOverlayOrder);
        AddLayer("System", UILayer.kSystemOrder);
    }

    protected override void OnDestroy()
    {
        if (m_UIRoot)
        {
            Destroy(m_UIRoot.gameObject);
            m_UIRoot = null!;
        }
    }

    public UILayer AddLayer(string name, int order)
    {
        var layer = m_UIRoot.MakeChild<UILayer>(name + " Layer");
        layer.Order = order;
        (m_Configure ?? DefaultConfigure)(layer, layer.Canvas);
        m_Layers.Add(order, layer);
        return layer;
    }

    public void Configure(Action<UILayer, Canvas>? configure)
    {
        m_Configure = configure;
        configure ??= m_Configure ?? DefaultConfigure;
        foreach (var (_, layer) in m_Layers)
        {
            configure(layer, layer.Canvas);
        }
    }

    public static void DefaultConfigure(UILayer layer, Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        if (canvas.TryGetComponent<CanvasScaler>(out var scaler))
        {
            Destroy(scaler);
        }
    }
}
