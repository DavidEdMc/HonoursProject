let lessonVolume = parseFloat(localStorage.getItem("lessonVolume") || "0.5");
let isMuted = localStorage.getItem("lessonMuted") === "true";

document.addEventListener("DOMContentLoaded", () => {
    const slider = document.getElementById("volumeSlider");
    const icon = document.getElementById("volumeIcon");

    // Apply saved mute state
    updateVolumeIcon();
    applyVolumeToAllSounds();
    updateSliderState();

    if (slider) {
        slider.value = lessonVolume;
        slider.style.setProperty("--value", slider.value);

        slider.addEventListener("input", () => {
            lessonVolume = parseFloat(slider.value);
            localStorage.setItem("lessonVolume", lessonVolume);
            slider.style.setProperty("--value", slider.value);

            applyVolumeToAllSounds();
        });
    }

    if (icon) {
        icon.addEventListener("click", () => {
            isMuted = !isMuted;
            localStorage.setItem("lessonMuted", isMuted);
            updateVolumeIcon();
            applyVolumeToAllSounds();
            updateSliderState();
        });
    }
});

function applyVolumeToAllSounds() {
    const soundIds = ["soundCorrect", "soundIncorrect", "soundComplete"];

    soundIds.forEach(id => {
        const audio = document.getElementById(id);
        if (audio) {
            audio.volume = isMuted ? 0 : lessonVolume;
        }
    });
}

function updateVolumeIcon() {
    const icon = document.getElementById("volumeIcon");
    if (!icon) return;

    icon.src = isMuted
        ? "/assets/images/mute.png"
        : "/assets/images/audio.png";
}

function updateSliderState() {
    const slider = document.getElementById("volumeSlider");
    if (!slider) return;

    if (isMuted) {
        slider.disabled = true;
        slider.classList.add("slider-disabled");
    } else {
        slider.disabled = false;
        slider.classList.remove("slider-disabled");
    }
}
