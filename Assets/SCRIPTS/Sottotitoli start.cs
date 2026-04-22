using UnityEngine;
using TMPro;

public class SubtitleSystem : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI subtitleText;

    [Header("Sottotitolo 1")]
    public float start1;
    public float end1;
    [TextArea(2, 3)] public string text1;

    [Header("Sottotitolo 2")]
    public float start2;
    public float end2;
    [TextArea(2, 3)] public string text2;

    [Header("Sottotitolo 3")]
    public float start3;
    public float end3;
    [TextArea(2, 3)] public string text3;

    [Header("Sottotitolo 4")]
    public float start4;
    public float end4;
    [TextArea(2, 3)] public string text4;

    [Header("Sottotitolo 5")]
    public float start5;
    public float end5;
    [TextArea(2, 3)] public string text5;

    [Header("Sottotitolo 6")]
    public float start6;
    public float end6;
    [TextArea(2, 3)] public string text6;

    [Header("Sottotitolo 7")]
    public float start7;
    public float end7;
    [TextArea(2, 3)] public string text7;

    [Header("Sottotitolo 8")]
    public float start8;
    public float end8;
    [TextArea(2, 3)] public string text8;

    [Header("Sottotitolo 9")]
    public float start9;
    public float end9;
    [TextArea(2, 3)] public string text9;

    [Header("Sottotitolo 10")]
    public float start10;
    public float end10;
    [TextArea(2, 3)] public string text10;

    [Header("Sottotitolo 11")]
    public float start11;
    public float end11;
    [TextArea(2, 3)] public string text11;

    [Header("Sottotitolo 12")]
    public float start12;
    public float end12;
    [TextArea(2, 3)] public string text12;

    [Header("Sottotitolo 13")]
    public float start13;
    public float end13;
    [TextArea(2, 3)] public string text13;

    [Header("Sottotitolo 14")]
    public float start14;
    public float end14;
    [TextArea(2, 3)] public string text14;

    [Header("Sottotitolo 15")]
    public float start15;
    public float end15;
    [TextArea(2, 3)] public string text15;

    [Header("Sottotitolo 16")]
    public float start16;
    public float end16;
    [TextArea(2, 3)] public string text16;

    [Header("Sottotitolo 17")]
    public float start17;
    public float end17;
    [TextArea(2, 3)] public string text17;

    [Header("Sottotitolo 18")]
    public float start18;
    public float end18;
    [TextArea(2, 3)] public string text18;

    [Header("Sottotitolo 19")]
    public float start19;
    public float end19;
    [TextArea(2, 3)] public string text19;

    [Header("Sottotitolo 20")]
    public float start20;
    public float end20;
    [TextArea(2, 3)] public string text20;

    private float _timer;

    void Start()
    {
        _timer = 0f;
        if (subtitleText != null)
            subtitleText.text = "";
    }

    void Update()
    {
        _timer += Time.deltaTime;

        string toShow = "";

        if (_timer >= start1  && _timer < end1  && !string.IsNullOrEmpty(text1))  toShow = text1;
        if (_timer >= start2  && _timer < end2  && !string.IsNullOrEmpty(text2))  toShow = text2;
        if (_timer >= start3  && _timer < end3  && !string.IsNullOrEmpty(text3))  toShow = text3;
        if (_timer >= start4  && _timer < end4  && !string.IsNullOrEmpty(text4))  toShow = text4;
        if (_timer >= start5  && _timer < end5  && !string.IsNullOrEmpty(text5))  toShow = text5;
        if (_timer >= start6  && _timer < end6  && !string.IsNullOrEmpty(text6))  toShow = text6;
        if (_timer >= start7  && _timer < end7  && !string.IsNullOrEmpty(text7))  toShow = text7;
        if (_timer >= start8  && _timer < end8  && !string.IsNullOrEmpty(text8))  toShow = text8;
        if (_timer >= start9  && _timer < end9  && !string.IsNullOrEmpty(text9))  toShow = text9;
        if (_timer >= start10 && _timer < end10 && !string.IsNullOrEmpty(text10)) toShow = text10;
        if (_timer >= start11 && _timer < end11 && !string.IsNullOrEmpty(text11)) toShow = text11;
        if (_timer >= start12 && _timer < end12 && !string.IsNullOrEmpty(text12)) toShow = text12;
        if (_timer >= start13 && _timer < end13 && !string.IsNullOrEmpty(text13)) toShow = text13;
        if (_timer >= start14 && _timer < end14 && !string.IsNullOrEmpty(text14)) toShow = text14;
        if (_timer >= start15 && _timer < end15 && !string.IsNullOrEmpty(text15)) toShow = text15;
        if (_timer >= start16 && _timer < end16 && !string.IsNullOrEmpty(text16)) toShow = text16;
        if (_timer >= start17 && _timer < end17 && !string.IsNullOrEmpty(text17)) toShow = text17;
        if (_timer >= start18 && _timer < end18 && !string.IsNullOrEmpty(text18)) toShow = text18;
        if (_timer >= start19 && _timer < end19 && !string.IsNullOrEmpty(text19)) toShow = text19;
        if (_timer >= start20 && _timer < end20 && !string.IsNullOrEmpty(text20)) toShow = text20;

        if (subtitleText != null)
            subtitleText.text = toShow;
    }
}