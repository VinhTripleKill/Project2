using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIDisplay : MonoBehaviour
{
    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    public void SetSprite(Sprite original)
    {
        if (original == null)
        {
            img.sprite = null;
            return;
        }

        Texture2D tex = original.texture;

        int width = tex.width;
        int height = tex.height;

        int size = Mathf.Min(width, height); // lấy cạnh nhỏ hơn để crop vuông

        // tính vị trí crop ở giữa
        int x = (width - size) / 2;
        int y = (height - size) / 2;

        Rect rect = new Rect(x, y, size, size);

        Sprite newSprite = Sprite.Create(
            tex,
            rect,
            new Vector2(0.5f, 0.5f)
        );

        img.sprite = newSprite;

        // optional: giữ pixel perfect
        img.SetNativeSize();
    }
}