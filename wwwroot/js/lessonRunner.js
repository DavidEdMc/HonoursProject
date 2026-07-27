function shuffleArray(arr) {
    return arr
        .map(value => ({ value, sort: Math.random() }))
        .sort((a, b) => a.sort - b.sort)
        .map(({ value }) => value);
}

const questions = shuffleArray(window.lessonData.questions);

let currentIndex = window.lessonData.currentIndex;
let totalQuestions = questions.length;
let selectedOptionId = null;
let selectedOptionIsCorrect = null;
let userMatches = {};
let hasSubmitted = false;
let currentQuestionId = null;
let attemptCount = 0;
let currentLessonId = window.lessonData.lessonId ?? window.lessonData.id;
let incorrectQuestions = [];
let correctQuestions = [];
let retryMode = false;


function loadQuestion(index) {
    const q = questions[index];
    currentQuestionId = q.id;
    attemptCount = 0;
    const block = document.getElementById("questionBlock");

    let html = `<h3>${q.question_text}</h3>`;

    if (q.question_type === "multiple_choice") {
        html += `<div class="mcq-options">`;
        shuffleArray(q.Options).forEach(opt => {
            html += `
                <button class="mcq-option"
                    data-id="${opt.id}"
                    onclick="selectOption(${opt.id}, ${opt.is_correct})">
                    ${opt.option_text}
                </button>`;
        });
        html += `</div>`;
    }
    else if (q.question_type === "fill_blank") {
        html += `
            <input id="fillInput" type="text" class="fillblank-input" placeholder="Type your answer..." />
        `;
    }
    else if (q.question_type === "spot_error") {
        html += `<div class="spoterror-options">`;

        shuffleArray(q.Options).forEach(opt => {
            html += `
                <button class="spoterror-option"
                    data-id="${opt.id}"
                    data-selected="false"
                    onclick="selectOption(${opt.id}, ${opt.is_correct})">
                    ${opt.option_text}
                </button>
            `;
        });

        html += `</div>`;
    }
    else if (q.question_type === "ordering") {
        html += `<ul id="orderingList" class="ordering-list">`;

        shuffleArray(q.Options).forEach(opt => {
            html += `
                <li class="ordering-item"
                    draggable="true"
                    data-id="${opt.id}"
                    data-order="${opt.order_position}">
                    ${opt.option_text}
                </li>
            `;
        });

        html += `</ul>`;
    }
    else if (q.question_type === "drag_drop_match") {
        const draggables = shuffleArray(q.Options);

        // Create one drop zone per unique match_key
        const uniqueKeys = [...new Set(q.Options.map(o => o.match_key))];
        const targets = uniqueKeys.map(key => ({ match_key: key }));

        html += `<div class="drag-drop-container">`;
        html += `<div class="drag-drop-left">`;
        draggables.forEach(opt => {
            html += `
                <div class="drag-item"
                    draggable="true"
                    data-id="${opt.id}"
                    data-key="${opt.match_key}">
                    ${opt.option_text}
                </div>
            `;
        });
        html += `</div>`;

        html += `<div class="drag-drop-right">`;
        targets.forEach(opt => {
            html += `
                <div class="drop-zone"
                    data-key="${opt.match_key}">
                    Drop here: ${opt.match_key}
                </div>
            `;
        });
        html += `</div>`;

        html += `</div>`;
    }
    
    block.innerHTML = html;

    if (q.question_type === "ordering") {
        document.querySelectorAll(".ordering-item").forEach(item => {
            item.addEventListener("dragstart", handleDragStart);
            item.addEventListener("dragover", handleDragOver);
            item.addEventListener("dragend", handleDragEnd);
        });
    }

    if (q.question_type === "drag_drop_match") {
        userMatches = {}; // reset matches

        // Draggable items
        document.querySelectorAll(".drag-item").forEach(item => {
            item.addEventListener("dragstart", ddDragStart);
        });

        // Drop zones
        document.querySelectorAll(".drop-zone").forEach(zone => {
            zone.addEventListener("dragover", ddDragOver);
            zone.addEventListener("dragleave", ddDragLeave);
            zone.addEventListener("drop", ddDrop);
        });
    }

    // Reset state
    selectedOptionId = null;
    selectedOptionIsCorrect = null;
    document.getElementById("feedback").innerHTML = "";
    document.getElementById("submitBtn").style.display = "inline-block";
    document.getElementById("nextBtn").style.display = "none";

    // Update header
    const header = document.getElementById("questionHeader");

    if (!retryMode) {
        header.textContent = `Question ${index + 1} of ${totalQuestions}`;
    } else {
        const retryIndex = incorrectQuestions.indexOf(currentIndex) + 1;
        const retryTotal = incorrectQuestions.length;

        header.textContent = `Retry ${retryIndex} of ${retryTotal}`;
    }

    updateProgressBar();
}

function handleDragStart(e) {
e.dataTransfer.setData("text/plain", e.target.dataset.id);
e.target.classList.add("dragging");
}

function handleDragOver(e) {
    e.preventDefault();
    const dragging = document.querySelector(".dragging");
    const list = document.getElementById("orderingList");
    const items = [...list.querySelectorAll(".ordering-item")];

    const afterElement = items.find(item => {
        const rect = item.getBoundingClientRect();
        return e.clientY < rect.top + rect.height / 2;
    });

    if (!afterElement) {
        list.appendChild(dragging);
    } else {
        list.insertBefore(dragging, afterElement);
    }
}

function handleDragEnd(e) {
    e.target.classList.remove("dragging");
}

function ddDragStart(e) {
    // Store BOTH the match_key and the visible text
    e.dataTransfer.setData("drag_key", e.target.dataset.key);
    e.dataTransfer.setData("drag_text", e.target.innerText.trim());

    e.target.classList.add("dragging");
}

function ddDragOver(e) {
    e.preventDefault();
    e.target.classList.add("drag-over");
}

function ddDragLeave(e) {
    e.target.classList.remove("drag-over");
}

function ddDrop(e) {
    e.preventDefault();
    e.target.classList.remove("drag-over");

    const dragKey = e.dataTransfer.getData("drag_key");      // draggable match_key
    const dragText = e.dataTransfer.getData("drag_text");    // draggable text
    const targetKey = e.target.dataset.key;                  // drop zone expected key

    // Store match
    userMatches[targetKey] = dragKey;

    // Remove previous validation styling
    e.target.classList.remove("correct", "incorrect");

    // Apply filled state
    e.target.classList.add("filled");

    // Visual feedback
    e.target.innerHTML = `${dragText} : ${targetKey}`;
}

function selectOption(optionId, isCorrect) {
    const btn = document.querySelector(`[data-id="${optionId}"]`);

    // MCQ: single-select behaviour
    if (btn.classList.contains("mcq-option")) {

        // Clear correctness + selected styling on ALL MCQ options
        document.querySelectorAll('.mcq-option').forEach(b => {
            b.classList.remove('correct', 'incorrect', 'selected');
        });

        // Store selection
        selectedOptionId = optionId;
        selectedOptionIsCorrect = isCorrect;

        // Apply selected highlight
        btn.classList.add('selected');
        return;
    }

    // SpotError: multi-select toggle behaviour
    if (btn.classList.contains("spoterror-option")) {
        // If user is starting a new attempt, clear previous selections
        if (hasSubmitted) {
            document.querySelectorAll(".spoterror-option").forEach(o => {
                o.classList.remove("selected", "correct", "incorrect");
                o.dataset.selected = "false";
            });
            hasSubmitted = false; // reset flag
        }

        // Toggle selection
        if (btn.dataset.selected === "true") {
            btn.dataset.selected = "false";
            btn.classList.remove("selected");
        } else {
            btn.dataset.selected = "true";
            btn.classList.add("selected");
        }
    }
}

function submitAnswer() {
    attemptCount++;
    const type = questions[currentIndex].question_type;

    if (type === "multiple_choice") {
        validateMCQ();
    }
    else if (type === "spot_error") {
        validateSpotError();
    }
    else if (type === "ordering") {
        validateOrdering();
    }
    else if (type === "fill_blank") {
        validateFillBlank();
    }
    else if (type === "drag_drop_match") {
        validateDragDropMatch();
    }
}

function validateMCQ() {
    const feedback = document.getElementById("feedback");

    if (!selectedOptionId) {
        feedback.innerHTML = "<span class='incorrect'>Please select an option.</span>";
        return;
    }

    const clickedBtn = document.querySelector(`button[data-id="${selectedOptionId}"]`);

    document.querySelectorAll('.mcq-option').forEach(b => {
        b.classList.remove('correct', 'incorrect');
    });

    if (selectedOptionIsCorrect) {
        feedback.innerHTML = "<span class='correct'>Correct!</span>";
        clickedBtn.classList.add('correct');

        recordAnswer(true);
    } else {
        feedback.innerHTML = "<span class='incorrect'>Incorrect!</span>";
        clickedBtn.classList.add('incorrect');

        recordAnswer(false);
    }

    sendAttemptToServer();

    disableAllAnswers();
    document.getElementById("nextBtn").style.display = "block";
    document.getElementById("submitBtn").style.display = "none";
}

function validateSpotError() {
    hasSubmitted = true;
    const feedback = document.getElementById("feedback");
    const options = document.querySelectorAll(".spoterror-option");
    const q = questions[currentIndex];

    currentQuestionId = q.id; 

    let anySelected = false;
    let allSelectedAreCorrect = true;
    let allCorrectAreSelected = true;

    options.forEach(opt => opt.classList.remove("correct", "incorrect"));

    options.forEach(opt => {
        const isSelected = opt.dataset.selected === "true";
        const optionId = parseInt(opt.dataset.id);
        const option = q.Options.find(o => o.id === optionId);

        if (isSelected) {
            anySelected = true;

            if (!option.is_correct) {
                opt.classList.add("correct");
            } else {
                opt.classList.add("incorrect");
                allSelectedAreCorrect = false;
            }
        }
    });

    q.Options.forEach(opt => {
        if (!opt.is_correct) {
            const btn = document.querySelector(`button[data-id="${opt.id}"]`);
            if (btn.dataset.selected !== "true") {
                allCorrectAreSelected = false;
            }
        }
    });

    if (!anySelected) {
        feedback.innerHTML = "<span class='incorrect'>Please select at least one statement.</span>";
        return;
    }

    if (allSelectedAreCorrect && allCorrectAreSelected) {
        feedback.innerHTML = "<span class='correct'>Correct — all incorrect statements were selected.</span>";
        selectedOptionIsCorrect = true;

        // Correct case — pick ANY correct option ID
        const firstIncorrect = q.Options.find(o => !o.is_correct);
        selectedOptionId = firstIncorrect.id;

        recordAnswer(true);
    } else {
        feedback.innerHTML = "<span class='incorrect'>You must select ALL incorrect statements and no correct ones.</span>";
        selectedOptionIsCorrect = false;

        // Incorrect case — fallback ID
        selectedOptionId = q.Options[0].id;

        recordAnswer(false);
    }

    sendAttemptToServer();

    disableAllAnswers();
    document.getElementById("nextBtn").style.display = "block";
    document.getElementById("submitBtn").style.display = "none";
}

function validateOrdering() {
    const items = document.querySelectorAll(".ordering-item");
    const feedback = document.getElementById("feedback");
    const q = questions[currentIndex];

    currentQuestionId = q.id;

    let allCorrect = true;

    items.forEach((item, index) => {
        const correctPos = parseInt(item.dataset.order);

        item.classList.remove("correct", "incorrect");

        if (correctPos === index + 1) {
            item.classList.add("correct");
        } else {
            item.classList.add("incorrect");
            allCorrect = false;
        }
    });

    if (allCorrect) {
        feedback.innerHTML = "<span class='correct'>Correct order!</span>";
        selectedOptionIsCorrect = true;

        // Correct case — any valid option ID
        selectedOptionId = q.Options[0].id;

        recordAnswer(true);
    } else {
        feedback.innerHTML = "<span class='incorrect'>Some items are in the wrong order.</span>";
        selectedOptionIsCorrect = false;

        // Incorrect case — fallback ID
        selectedOptionId = q.Options[0].id;

        recordAnswer(false);
    }

    sendAttemptToServer();

    disableAllAnswers();
    document.getElementById("nextBtn").style.display = "block";
    document.getElementById("submitBtn").style.display = "none";
}

function validateFillBlank() {
    const input = document.getElementById("fillInput");
    const feedback = document.getElementById("feedback");

    const q = questions[currentIndex];
    currentQuestionId = q.id; 

    const userAnswer = input.value.trim().toLowerCase();
    const correctOption = q.Options.find(o => o.is_correct);
    const correctAnswer = correctOption.option_text.toLowerCase();

    input.classList.remove("correct", "incorrect");

    if (userAnswer === "") {
        feedback.innerHTML = "<span class='incorrect'>Please enter an answer.</span>";
        return;
    }

    if (userAnswer === correctAnswer) {
        feedback.innerHTML = "<span class='correct'>Correct!</span>";
        input.classList.add("correct");

        // Correct case
        selectedOptionIsCorrect = true;
        selectedOptionId = correctOption.id;

        recordAnswer(true);
    } else {
        feedback.innerHTML = "<span class='incorrect'>Incorrect.</span>";
        input.classList.add("incorrect");

        selectedOptionIsCorrect = false;

        // Incorrect case — fallback ID
        selectedOptionId = q.Options[0].id;

        recordAnswer(false);
    }

    sendAttemptToServer();

    disableAllAnswers();
    document.getElementById("nextBtn").style.display = "block";
    document.getElementById("submitBtn").style.display = "none";
}

function validateDragDropMatch() {
    const feedback = document.getElementById("feedback");
    const q = questions[currentIndex];

    currentQuestionId = q.id;

    let allCorrect = true;

    q.Options.forEach(opt => {
        const correctKey = opt.match_key;
        const userKey = userMatches[correctKey];

        if (correctKey !== userKey) {
            allCorrect = false;
        }
    });

    document.querySelectorAll('.drop-zone').forEach(zone => {
        const expectedKey = zone.dataset.key;
        const userKey = userMatches[expectedKey];

        zone.classList.remove("correct", "incorrect");

        if (userKey === expectedKey) {
            zone.classList.add("correct");
        } else {
            zone.classList.add("incorrect");
        }
    });

    if (allCorrect) {
        feedback.innerHTML = "<span class='correct'>Correct!</span>";
        selectedOptionIsCorrect = true;

        // Correct case — any valid option ID
        selectedOptionId = q.Options[0].id;

        recordAnswer(true);
    } else {
        feedback.innerHTML = "<span class='incorrect'>Some matches are incorrect.</span>";
        selectedOptionIsCorrect = false;

        // Incorrect case — fallback ID
        selectedOptionId = q.Options[0].id;

        recordAnswer(false);
    }

    sendAttemptToServer();

    disableAllAnswers();
    document.getElementById("nextBtn").style.display = "block";
    document.getElementById("submitBtn").style.display = "none";
}

function recordAnswer(isCorrect) {
    if (!retryMode) {
        // FIRST PASS
        if (isCorrect) {
            correctQuestions.push(currentIndex);
        } else {
            incorrectQuestions.push(currentIndex);
        }
    } else {
        // RETRY MODE
        if (isCorrect) {
            incorrectQuestions = incorrectQuestions.filter(i => i !== currentIndex);
            if (!correctQuestions.includes(currentIndex)) {
                correctQuestions.push(currentIndex);
            }
        }
    }
}

function nextQuestion() {
    // Hide next button, show submit button again
    document.getElementById("nextBtn").style.display = "none";
    document.getElementById("submitBtn").style.display = "inline-block";

    // -----------------------------
    // FIRST PASS (retryMode == false)
    // -----------------------------
    if (!retryMode) {

        currentIndex++;

        // Still in first pass
        if (currentIndex < totalQuestions) {
            loadQuestion(currentIndex);
            return;
        }

        // First pass finished → enter retry mode
        retryMode = true;

        // If there are incorrect questions, start retrying them
        if (incorrectQuestions.length > 0) {
            currentIndex = incorrectQuestions[0];
            loadQuestion(currentIndex);
            return;
        }

        // No incorrect questions → lesson complete
        finishLesson();
        return;
    }

    // -----------------------------
    // RETRY MODE (retryMode == true)
    // -----------------------------
    // Find current question's position in the incorrect list
    const idx = incorrectQuestions.indexOf(currentIndex);

    // Move to next incorrect question
    if (idx !== -1 && idx + 1 < incorrectQuestions.length) {
        currentIndex = incorrectQuestions[idx + 1];
        loadQuestion(currentIndex);
        return;
    }

    // End of incorrect list → check if any remain
    if (incorrectQuestions.length > 0) {
        // Loop back to start of incorrect list
        currentIndex = incorrectQuestions[0];
        loadQuestion(currentIndex);
        return;
    }

    // All questions mastered → finish lesson
    finishLesson();
}

function finishLesson() {
    const bar = document.getElementById("lessonProgressBar");
    const banner = document.getElementById("lessonCompleteBanner");

    bar.style.width = "100%";
    bar.style.animation = "progressPulse 0.8s ease-in-out 2";
    banner.classList.add("show");

    setTimeout(() => {
        window.location.href = "/Lesson/Complete";
    }, 1200);
}


function disableAllAnswers() {
    // MCQ
    document.querySelectorAll('.mcq-option').forEach(btn => {
        btn.disabled = true;
    });

    // SpotError
    document.querySelectorAll('.spoterror-option').forEach(btn => {
        btn.disabled = true;
    });

    // Ordering
    document.querySelectorAll('.ordering-item').forEach(item => {
        item.setAttribute("draggable", false);
    });

    // FillBlank
    const fillInput = document.getElementById("fillInput");
    if (fillInput) fillInput.disabled = true;

    // DragDropMatch
    document.querySelectorAll('.drag-item').forEach(d => {
        d.setAttribute("draggable", false);

        // Remove drag event listeners
        d.removeEventListener("dragstart", ddDragStart);

        // Visual disabled styling
        d.classList.add("disabled");
    });

    document.querySelectorAll('.drop-zone').forEach(zone => {
        // Remove drop listeners
        zone.removeEventListener("dragover", ddDragOver);
        zone.removeEventListener("dragleave", ddDragLeave);
        zone.removeEventListener("drop", ddDrop);

        // Visual disabled styling
        zone.classList.add("disabled");
    });
}

function updateProgressBar() {
    let percent;

    if (!retryMode) {
        // FIRST PASS — normal behaviour
        percent = ((currentIndex + 1) / totalQuestions) * 100;
    } else {
        // RETRY MODE — progress based on incorrect questions
        const retryIndex = incorrectQuestions.indexOf(currentIndex) + 1;
        const retryTotal = incorrectQuestions.length;

        percent = (retryIndex / retryTotal) * 100;
    }

    document.getElementById("lessonProgressBar").style.width = percent + "%";
}


updateProgressBar();

function sendAttemptToServer() {
    console.log("Sending attempt payload:", {
        lessonId: currentLessonId,
        questionId: currentQuestionId,
        optionId: selectedOptionId,
        isCorrect: selectedOptionIsCorrect,
        timeTakenSeconds: 0,
        attempts: attemptCount
    });

    const payload = {
        lessonId: currentLessonId,
        questionId: currentQuestionId,
        optionId: selectedOptionId,
        isCorrect: selectedOptionIsCorrect,
        timeTakenSeconds: 0,
        attempts: attemptCount
    };

    fetch('/Lesson/RecordAttempt', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });
}

window.onload = () => {
    loadQuestion(currentIndex);
};