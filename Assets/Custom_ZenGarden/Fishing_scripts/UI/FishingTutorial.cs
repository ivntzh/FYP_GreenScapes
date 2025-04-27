using UnityEngine;
using UnityEngine.UI;

public class FishingTutorial: MonoBehaviour
{
    [Header("Assign your page panels here, in order")]
    public GameObject[] pages;

    [Header("Optional: if you want to disable navigation when tutorial closed")]
    public GameObject navigationRoot; // e.g. a parent for Next/Back/End buttons

    int currentPage = -1; // -1 = tutorial closed

    void Start()
    {
        // Manually wire each panel's buttons
        for (int i = 0; i < pages.Length; i++)
        {
            var panel = pages[i].transform;

            // Next button
            var next = panel.Find("Next btn")?.GetComponent<Button>();
            if (next != null)
            {
                next.onClick.RemoveAllListeners();
                next.onClick.AddListener(NextPage);
                next.interactable = (i < pages.Length - 1);
            }

            // Back button
            var back = panel.Find("Back btn")?.GetComponent<Button>();
            if (back != null)
            {
                back.onClick.RemoveAllListeners();
                back.onClick.AddListener(PreviousPage);
                back.interactable = (i > 0);
            }

            // End button (only on last)
            var end = panel.Find("End btn")?.GetComponent<Button>();
            if (end != null)
            {
                end.onClick.RemoveAllListeners();
                end.onClick.AddListener(EndTutorial);
                end.gameObject.SetActive(i == pages.Length - 1);
            }

            // Hide all pages initially
            pages[i].SetActive(false);
        }
    }

    /// <summary>
    /// Called by your "How To Play" button.
    /// Opens page 0 if closed; otherwise closes the tutorial.
    /// </summary>
    public void ToggleTutorial()
    {
        if (currentPage == -1)
            ShowPage(0);
        else
            EndTutorial();
    }

    /// <summary>Go to the next page (if any).</summary>
    public void NextPage()
    {
        if (currentPage < 0 || currentPage >= pages.Length - 1) return;
        ShowPage(currentPage + 1);
    }

    /// <summary>Go to the previous page (if any).</summary>
    public void PreviousPage()
    {
        if (currentPage <= 0) return;
        ShowPage(currentPage - 1);
    }

    /// <summary>Closes all panels and resets state.</summary>
    public void EndTutorial()
    {
        if (currentPage >= 0 && currentPage < pages.Length)
            pages[currentPage].SetActive(false);

        currentPage = -1;
        UpdateNavigationVisibility();
    }

    /// <summary>Internal: show exactly one page, hide the rest.</summary>
    void ShowPage(int pageIndex)
    {
        // Hide old
        if (currentPage >= 0 && currentPage < pages.Length)
            pages[currentPage].SetActive(false);

        // Show new
        currentPage = pageIndex;
        pages[currentPage].SetActive(true);

        UpdateNavigationVisibility();
    }

    /// <summary>
    /// Enable/disable your navigation container if desired.
    /// You can also drive individual button interactability here.
    /// </summary>
    void UpdateNavigationVisibility()
    {
        if (navigationRoot != null)
            navigationRoot.SetActive(currentPage != -1);
    }

    // (Optional) you can also expose methods to check if Next/Back should be interactable,
    // or drive UI transitions from here rather than via the Inspector.
}
