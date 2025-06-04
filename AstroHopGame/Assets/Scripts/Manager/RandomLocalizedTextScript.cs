using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;

[RequireComponent(typeof(LocalizeStringEvent))]
public class RandomLocalizedText : MonoBehaviour
{
    public string alternativeEntryKey;
    private LocalizeStringEvent _locStringEvent;

    void Awake()
    {
        _locStringEvent = GetComponent<LocalizeStringEvent>();

        if (Random.value < 0.5f)
        {
            var ls = _locStringEvent.StringReference;
            ls.TableEntryReference = alternativeEntryKey;
            _locStringEvent.StringReference = ls;
        }
    }
}
