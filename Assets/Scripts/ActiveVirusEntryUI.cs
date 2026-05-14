using System.Text;
using UnityEngine;

public class ActiveVirusEntryUI : MonoBehaviour
{
    private const string EntryKey = "your_virus";

    [Header("Dependencies")]
    [SerializeField] private ActiveVirusManager activeVirusManager;
    [SerializeField] private ContentManager contentManager;

    [Header("Description Template")]
    [TextArea(2, 4)]
    [SerializeField]
    private string descriptionText =
        "A custom virus you have released into the village. " +
        "Patients infected by it show a specific combination of symptoms that you designed.";

    private bool lastActiveState;
    private VirusUpgradeSO[] lastSourceUpgrades;

    private void Update()
    {
        if (activeVirusManager == null || contentManager == null) return;

        bool shouldShow = activeVirusManager.HasPlayerVirusActive;

        if (shouldShow != lastActiveState)
        {
            Debug.Log($"[ActiveVirusEntry] State changed. shouldShow={shouldShow}");
            if (shouldShow)
            {
                AddOrRefreshEntry();
                Debug.Log("[ActiveVirusEntry] Entry added.");
            }
            else
            {
                contentManager.RemoveEntry(EntryKey);
                lastSourceUpgrades = null;
            }
            lastActiveState = shouldShow;
        }
        else if (shouldShow)
        {
            if (HasSourceUpgradesChanged())
                AddOrRefreshEntry();
        }
    }

    private bool HasSourceUpgradesChanged()
    {
        var current = activeVirusManager.CurrentVirus != null
            ? activeVirusManager.CurrentVirus.sourceUpgrades
            : null;

        if (current == null) return false;
        if (lastSourceUpgrades == null) return true;
        if (current.Count != lastSourceUpgrades.Length) return true;

        for (int i = 0; i < current.Count; i++)
        {
            if (current[i] != lastSourceUpgrades[i]) return true;
        }
        return false;
    }

    private void AddOrRefreshEntry()
    {
        if (activeVirusManager.CurrentVirus == null) return;
        var virus = activeVirusManager.CurrentVirus;

        lastSourceUpgrades = virus.sourceUpgrades != null
            ? virus.sourceUpgrades.ToArray()
            : new VirusUpgradeSO[0];

        var sb = new StringBuilder();
        sb.Append("<size=140%><b>Your Virus</b></size>\n");
        sb.Append(descriptionText);
        sb.Append("\n\n");

        sb.Append("<b>Main symptoms:</b> ");
        if (virus.sourceUpgrades != null && virus.sourceUpgrades.Count > 0)
        {
            for (int i = 0; i < virus.sourceUpgrades.Count; i++)
            {
                if (virus.sourceUpgrades[i] != null)
                    sb.Append(virus.sourceUpgrades[i].shortName);

                if (i < virus.sourceUpgrades.Count - 1)
                    sb.Append("; ");
            }
            sb.Append(".");
        }
        else
        {
            sb.Append("(unknown)");
        }
        sb.Append("\n");

        sb.Append("<b>Lethality:</b> ");
        sb.Append((virus.lethalityPerDay * 100f).ToString("0"));
        sb.Append("%/day · ");

        sb.Append("<b>Daily spread:</b> +");
        sb.Append(virus.dailyInfectionsCap);
        sb.Append(" · ");

        sb.Append("<b>Total spread:</b> +");
        sb.Append(virus.totalInfectionsBudget);

        contentManager.AddOrUpdateEntry(EntryKey, sb.ToString());
    }
}