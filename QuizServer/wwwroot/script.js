document.addEventListener("DOMContentLoaded", function () {
    const categorySelect = document.getElementById("category");
    const startBtn = document.getElementById("start-btn");
    const questionContainer = document.getElementById("question-container");
    const questionEl = document.getElementById("question");
    const answersEl = document.getElementById("answers");

    // HUD elements
    const scoreEl = document.getElementById("score");
    const feedbackEl = document.getElementById("feedback");

    let questions = [];
    let currentQuestionIndex = 0;
    let score = 0;
    let answeredCount = 0; // ✅ track how many questions have been answered
    let questionLocked = false; // prevents double clicks while showing feedback

    async function loadCategories() {
        try {
            const response = await fetch("/api/categories");
            if (!response.ok) throw new Error("Failed to load categories");
            const categories = await response.json();

            // Clear & add placeholder
            categorySelect.innerHTML = "";
            const placeholder = document.createElement("option");
            placeholder.value = "";
            placeholder.textContent = "Select category";
            placeholder.disabled = true;
            placeholder.selected = true;
            categorySelect.appendChild(placeholder);

            // Expecting array of strings (["biology", "world history", "geography"])
            categories.forEach(cat => {
                const option = document.createElement("option");
                option.value = cat;
                option.textContent = cat;
                categorySelect.appendChild(option);
            });
        } catch (error) {
            console.error("Error loading categories:", error);
            alert("Could not load categories.");
        }
    }

    async function loadQuestions(category) {
        // Reset UI for new run
        feedbackEl.textContent = "";
        score = 0;
        answeredCount = 0; // ✅ reset answered counter
        updateScore();
        currentQuestionIndex = 0;

        try {
            const response = await fetch(`/api/questions/byCategory/${encodeURIComponent(category)}`);
            if (!response.ok) throw new Error("Failed to load questions");
            questions = await response.json();

            if (!Array.isArray(questions) || questions.length === 0) {
                questionContainer.style.display = "none";
                alert("No questions found for this category.");
                return;
            }

            showQuestion();
            questionContainer.style.display = "block";
        } catch (err) {
            console.error("Error loading questions:", err);
            alert("Error loading questions. Please try again.");
        }
    }

    function updateScore() {
        // ✅ Show: "Score: 3 out 4 - 75% correct answers"
        const pct = answeredCount > 0 ? Math.round((score / answeredCount) * 100) : 0;
        scoreEl.textContent = `Score: ${score} out ${answeredCount} - ${pct}% correct answers`;
    }

    function clearFeedback() {
        feedbackEl.textContent = "";
    }

    function showQuestion() {
        if (currentQuestionIndex >= questions.length) {
            // Quiz finished
            questionEl.textContent = "Quiz completed!";
            answersEl.innerHTML = "";
            feedbackEl.textContent = `You scored ${score} out of ${questions.length}.`;
            return;
        }

        // Prepare next question
        clearFeedback();
        questionLocked = false;

        const q = questions[currentQuestionIndex];
        questionEl.textContent = q.text;
        answersEl.innerHTML = "";

        // Render answer buttons
        q.possibleAnswers.forEach((answer, index) => {
            const btn = document.createElement("button");
            btn.className = "answer-btn";
            btn.textContent = answer;

            btn.addEventListener("click", () => {
                if (questionLocked) return;
                questionLocked = true;

                // Mark selected
                btn.classList.add("selected");

                const isCorrect = index === q.correctAnswer;

                // Mark correctness
                if (isCorrect) {
                    btn.classList.add("correct");
                    feedbackEl.textContent = "Correct!";
                    score += 1;
                } else {
                    btn.classList.add("wrong");
                    feedbackEl.textContent = `Wrong! Correct answer: ${q.possibleAnswers[q.correctAnswer]}`;
                    // also highlight the correct one
                    const all = Array.from(answersEl.querySelectorAll(".answer-btn"));
                    const correctBtn = all[q.correctAnswer];
                    if (correctBtn) correctBtn.classList.add("correct");
                }

                // ✅ Count this question as answered and update HUD
                answeredCount += 1;
                updateScore();

                // Disable all buttons while we auto-advance
                Array.from(answersEl.children).forEach(b => (b.disabled = true));

                // Auto-go to next question after a short pause
                setTimeout(() => {
                    currentQuestionIndex++;
                    showQuestion();
                }, 1000);
            });

            answersEl.appendChild(btn);
        });
    }

    // Start quiz click
    startBtn.addEventListener("click", () => {
        const selectedCategory = categorySelect.value;
        if (!selectedCategory) {
            alert("Please select a category");
            return;
        }
        loadQuestions(selectedCategory);
    });

    // Initial load
    loadCategories();
});
