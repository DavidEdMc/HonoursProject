function waitForPopup() {
    return new Promise(resolve => {
        const check = () => {
            if (document.getElementById("achievement-popup")) resolve();
            else setTimeout(check, 50);
        };
        check();
    });
}

window.addEventListener("load", async () => {
    await waitForPopup();

    const stored = localStorage.getItem("pendingAchievements");
    if (!stored) return;

    const list = JSON.parse(stored);
    if (!list || list.length === 0) return;

    showAchievementQueue(list);
    localStorage.removeItem("pendingAchievements");
});


async function showAchievementQueue(list) {
    const popup = document.getElementById("achievement-popup");
    const nameEl = document.getElementById("ach-name");
    const descEl = document.getElementById("ach-desc");

    for (const a of list) {
        nameEl.textContent = a.Name;
        descEl.textContent = a.Description;

        popup.style.display = "block";
        popup.style.opacity = "1";

        await new Promise(resolve => setTimeout(resolve, 3000));

        popup.style.opacity = "0";
        await new Promise(resolve => setTimeout(resolve, 500));
    }

    popup.style.display = "none";
}