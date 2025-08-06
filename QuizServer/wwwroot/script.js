document.addEventListener("DOMContentLoaded", function () {
    const categorySelect = document.getElementById("category");
    const startBtn = document.getElementById("start-btn");
    const questionContainer = document.getElementById("question-container");
    const questionEl = document.getElementById("question");
    const answersEl = document.getElementById("answers");

    let questions = [];
    let currentQuestionIndex = 0;

    async function loadCategories() {
        try {
            const response = await fetch("/api/categories");
            const categories = await response.json();

            categorySelect.innerHTML = ""; // clear previous options

            // Add placeholder option
            const placeholderOption = document.createElement("option");
            placeholderOption.textContent = "Select category";
            placeholderOption.disabled = true;
            placeholderOption.selected = true;
            categorySelect.appendChild(placeholderOption);

            // Add options from category list
            categories.forEach(cat => {
                const option = document.createElement("option");
                option.value = cat; // string values like "Math"
                option.textContent = cat;
                categorySelect.appendChild(option);
            });
        } catch (error) {
            console.error("Error loading categories:", error);
        }
    }

    async function loadQuestions(category) {
        try {
            const response = await fetch(`/api/questions/byCategory/${encodeURIComponent(category)}`);
            if (!response.ok) throw new Error("Failed to load questions");

            questions = await response.json();
            if (questions.length === 0) {
                questionEl.textContent = "No questions found in this category.";
                answersEl.innerHTML = "";
                questionContainer.style.display = "block";
                return;
            }
            currentQuestionIndex = 0;
            showQuestion();
        } catch (error) {
            console.error("Error loading questions:", error);
            questionEl.textContent = "Error loading questions. Please try again.";
            answersEl.innerHTML = "";
            questionContainer.style.display = "block";
        }
    }

    function showQuestion() {
        if (currentQuestionIndex >= questions.length) {
            questionEl.textContent = "Quiz completed!";
            answersEl.innerHTML = "";
            return;
        }

        const q = questions[currentQuestionIndex];
        questionEl.textContent = q.text;
        answersEl.innerHTML = "";

        q.possibleAnswers.forEach((answer, index) => {
            const btn = document.createElement("button");
            btn.textContent = answer;
            btn.addEventListener("click", () => checkAnswer(index, q.correctAnswer));
            answersEl.appendChild(btn);
        });

        questionContainer.style.display = "block";
    }

    function checkAnswer(selected, correct) {
        if (selected === correct) {
            alert("Correct!");
        } else {
            alert("Wrong!");
        }
        currentQuestionIndex++;
        showQuestion();
    }

    startBtn.addEventListener("click", () => {
        const selectedCategory = categorySelect.value;
        if (!selectedCategory) {
            alert("Please select a category");
            return;
        }
        loadQuestions(selectedCategory);
    });

    loadCategories();
});
