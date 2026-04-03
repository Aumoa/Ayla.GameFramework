using System;
using UnityEngine;

namespace Ayla;

[RequireComponent(typeof(Canvas))]
public class UILayer : MonoBehaviour
{
    public const int kUserOrder = 0;
    public const int kOverlayOrder = 1000;
    public const int kSystemOrder = 10000;

    private Canvas? m_Canvas;
    private int m_Order;

    public Canvas Canvas
    {
        get
        {
            if (m_Canvas == null)
            {
                TryGetComponent(out m_Canvas);
            }

            if (m_Canvas == null)
            {
                throw new InvalidOperationException("Canvas component is missing.");
            }

            return m_Canvas;
        }
    }

    public int Order
    {
        get => m_Order;
        internal set
        {
            m_Order = value;
            Canvas.sortingOrder = value;
        }
    }
}
