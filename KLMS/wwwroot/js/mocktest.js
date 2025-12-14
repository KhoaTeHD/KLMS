// ========================================
// MOCK TEST - JAVASCRIPT
// Timer, Navigation, AJAX, Auto-save
// ========================================

let attemptId = 0;
let timeRemaining = 0;
let timerInterval = null;
let autoSaveInterval = null;
let totalQuestions = 0;
let answeredCount = 0;
let currentQuestionNumber = 1;
let contentData = [];
let isSubmitting = false;

// ========================================
// INITIALIZATION
// ========================================

$(document).ready(function () {
    initializeData();
    initializeTimer();
    initializeAutoSave();
    initializeEventHandlers();
    initializeNavigation();
    loadContentForQuestion(1);
    updateConnectionStatus();
    
    // Warning before leaving page
    window.addEventListener('beforeunload', function (e) {
        if (!isSubmitting && timeRemaining > 0) {
            e.preventDefault();
            e.returnValue = 'Bạn có thay đổi chưa lưu. Bạn có chắc muốn thoát?';
            return e.returnValue;
        }
    });
});

function initializeData() {
    attemptId = parseInt($('#attemptId').val());
    timeRemaining = parseInt($('#initialTimeRemaining').val());
    totalQuestions = parseInt($('#totalQuestions').val());
    answeredCount = parseInt($('#answeredCount').text());
    
    // Load content data
    const contentJson = $('#contentData').text();
    if (contentJson) {
        contentData = JSON.parse(contentJson);
    }
}

// ========================================
// TIMER - COUNTDOWN REALTIME
// ========================================

function initializeTimer() {
    updateTimerDisplay();
    
    // Update every 1 second
    timerInterval = setInterval(function () {
        timeRemaining--;
        updateTimerDisplay();
        
        // Warning at 5 minutes
        if (timeRemaining === 300) {
            showWarning('Còn 5 phút!', 'Bạn còn 5 phút để hoàn thành bài thi.');
        }
        
        // Warning at 1 minute
        if (timeRemaining === 60) {
            showWarning('Còn 1 phút!', 'Bạn còn 1 phút để hoàn thành bài thi.');
        }
        
        // Auto submit when time's up
        if (timeRemaining <= 0) {
            clearInterval(timerInterval);
            autoSubmitTest();
        }
    }, 1000);
}

function updateTimerDisplay() {
    const minutes = Math.floor(timeRemaining / 60);
    const seconds = timeRemaining % 60;
    const display = `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
    
    $('#timer').text(display);
    
    // Change color based on time remaining
    if (timeRemaining < 300) {
        $('#timer').addClass('text-danger').removeClass('text-warning');
    } else if (timeRemaining < 600) {
        $('#timer').addClass('text-warning').removeClass('text-danger');
    }
}

// ========================================
// AUTO-SAVE PROGRESS
// ========================================

function initializeAutoSave() {
    // Auto-save every 30 seconds
    autoSaveInterval = setInterval(function () {
        syncTimeRemaining();
    }, 30000);
}

function syncTimeRemaining() {
    $.ajax({
        url: '/MockTest/GetTimeRemaining',
        method: 'GET',
        data: { attemptId: attemptId },
        success: function (response) {
            if (response.success) {
                if (response.autoSubmitted) {
                    showWarning('Hết giờ!', 'Bài thi đã được tự động nộp.');
                    setTimeout(() => {
                        window.location.href = `/MockTest/TestResult/${attemptId}`;
                    }, 2000);
                }
            }
        }
    });
}

// ========================================
// EVENT HANDLERS
// ========================================

function initializeEventHandlers() {
    // Answer selection
    $('.answer-radio').on('change', function () {
        const questionId = $(this).data('question-id');
        const selectedAnswer = $(this).val();
        saveAnswer(questionId, selectedAnswer);
    });
    
    // Bookmark toggle
    $('.btn-bookmark').on('click', function () {
        const questionId = $(this).data('question-id');
        const isBookmarked = $(this).hasClass('active');
        toggleBookmark(questionId, !isBookmarked, $(this));
    });
    
    // Question number buttons - Use event delegation for better reliability
    $(document).on('click', '.btn-question-number', function (e) {
        e.preventDefault();
        const questionNumber = parseInt($(this).data('question-number'));
        //console.log('Clicked question number:', questionNumber);
        jumpToQuestion(questionNumber);
    });
    
    // Navigation buttons
    $('#btnPrevious').on('click', function (e) {
        e.preventDefault();
        console.log('Previous clicked, current:', currentQuestionNumber);
        if (currentQuestionNumber > 1) {
            jumpToQuestion(currentQuestionNumber - 1);
        }
    });
    
    $('#btnNext').on('click', function (e) {
        e.preventDefault();
        console.log('Next clicked, current:', currentQuestionNumber);
        if (currentQuestionNumber < totalQuestions) {
            jumpToQuestion(currentQuestionNumber + 1);
        }
    });
    
    // Save draft button
    $('#btnSaveDraft').on('click', function () {
        saveDraft();
    });
    
    // Refresh button
    $('#btnRefresh').on('click', function () {
        location.reload();
    });
    
    // Submit button
    $('#btnSubmitTest').on('click', function () {
        showSubmitConfirmation();
    });
    
    // Confirm submit
    $('#btnConfirmSubmit').on('click', function () {
        submitTest();
    });
    
    // Fullscreen button
    $('#btnFullscreen').on('click', function () {
        toggleFullscreen();
    });
    
    console.log('Event handlers initialized');
    console.log('Total questions:', totalQuestions);
    console.log('Question buttons found:', $('.btn-question-number').length);
}

// ========================================
// SAVE ANSWER
// ========================================

function saveAnswer(questionId, selectedAnswer) {
    $.ajax({
        url: '/MockTest/SaveAnswer',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            testAttemptId: attemptId,
            questionId: questionId,
            selectedAnswer: selectedAnswer,
            isBookmarked: null
        }),
        success: function (response) {
            if (response.success) {
                // Update UI
                answeredCount = response.totalAnswered;
                $('#answeredCount').text(answeredCount);
                
                // Update question button color
                const questionNumber = $(`.answer-radio[data-question-id="${questionId}"]`)
                    .closest('.question-item')
                    .data('question-number');
                $(`.btn-question-number[data-question-number="${questionNumber}"]`).addClass('answered');
                
                // Show brief success indicator
                showSuccessIndicator();
            } else {
                showError(response.message);
            }
        },
        error: function () {
            showError('Không thể lưu câu trả lời. Vui lòng thử lại.');
        }
    });
}

// ========================================
// BOOKMARK
// ========================================

function toggleBookmark(questionId, isBookmarked, $button) {
    $.ajax({
        url: '/MockTest/SaveAnswer',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            testAttemptId: attemptId,
            questionId: questionId,
            selectedAnswer: null,
            isBookmarked: isBookmarked
        }),
        success: function (response) {
            if (response.success) {
                // Update button
                $button.toggleClass('active', isBookmarked);
                
                // Update footer button
                const questionNumber = $(`.answer-radio[data-question-id="${questionId}"]`)
                    .closest('.question-item')
                    .data('question-number');
                $(`.btn-question-number[data-question-number="${questionNumber}"]`)
                    .toggleClass('bookmarked', isBookmarked);
            }
        }
    });
}

// ========================================
// NAVIGATION
// ========================================

function initializeNavigation() {
    // Scroll spy to update active question
    const $questionsContainer = $('#questionsContainer');
    $questionsContainer.on('scroll', function () {
        updateActiveQuestion();
    });
}

function jumpToQuestion(questionNumber) {
    //console.log('Jumping to question:', questionNumber);
    currentQuestionNumber = questionNumber;
    
    // Scroll to question in right column
    const $question = $(`#question-${questionNumber}`);
    const $container = $('#questionsContainer');
    
    if ($question.length && $container.length) {
        const containerElement = $container[0];
        const questionElement = $question[0];
        
        //console.log('Container found:', containerElement);
        //console.log('Question element found:', questionElement);
        
        // Method 1: Try scrollIntoView first (most reliable)
        try {
            questionElement.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
                inline: 'nearest'
            });
        } catch (e) {
            console.log('scrollIntoView failed, trying alternative:', e);
            
            // Method 2: Fallback to manual scroll calculation
            const questionOffsetTop = questionElement.offsetTop;
            const targetScrollTop = questionOffsetTop - 20; // 20px padding from top
            
            // Use jQuery animate as fallback
            $container.animate({
                scrollTop: targetScrollTop
            }, 300);
        }
    } else {
        console.warn('Question or container not found!');
        console.log('Question selector:', `#question-${questionNumber}`);
        console.log('Question exists:', $question.length);
        console.log('Container exists:', $container.length);
    }
    
    // Load content for this question
    loadContentForQuestion(questionNumber);
    
    // Update active state
    $('.btn-question-number').removeClass('active');
    $(`.btn-question-number[data-question-number="${questionNumber}"]`).addClass('active');
}

function updateActiveQuestion() {
    const container = $('#questionsContainer');
    const scrollTop = container.scrollTop();
    const containerTop = container.offset().top;
    
    $('.question-item').each(function () {
        const $this = $(this);
        const top = $this.offset().top - containerTop + scrollTop;
        const bottom = top + $this.outerHeight();
        
        if (scrollTop >= top && scrollTop < bottom) {
            currentQuestionNumber = $this.data('question-number');
            loadContentForQuestion(currentQuestionNumber);
            return false; // break
        }
    });
}

// ========================================
// CONTENT DISPLAY
// ========================================

function loadContentForQuestion(questionNumber) {
    const questionData = contentData.find(q => q.questionNumber === questionNumber);
    if (!questionData) return;
    
    const $display = $('#contentDisplay');
    
    // Determine which content to show
    let contentToShow = null;
    
    if (questionData.groupId !== null) {
        // Shared content - get the first question in this group
        const firstInGroup = contentData.find(q => q.groupId === questionData.groupId);
        contentToShow = firstInGroup;
    } else {
        // Individual content
        contentToShow = questionData;
    }
    
    if (!contentToShow || contentToShow.contentType === 'None') {
        $display.html(`
            <div class="text-center text-muted py-5">
                <i class="bi bi-file-text" style="font-size: 3rem;"></i>
                <p class="mt-3">Không có nội dung đề bài</p>
            </div>
        `);
        return;
    }
    
    // Display content based on type
    switch (contentToShow.contentType) {
        case 'Text':
            displayTextContent(contentToShow.contentData);
            break;
        case 'Image':
            displayImageContent(contentToShow.contentData);
            break;
        case 'PDF':
            displayPDFContent(contentToShow.contentData);
            break;
        case 'Video':
            displayVideoContent(contentToShow.contentData);
            break;
        default:
            $display.html(`
                <div class="text-center text-muted py-5">
                    <p>Loại nội dung không được hỗ trợ</p>
                </div>
            `);
    }
}

function displayTextContent(content) {
    $('#contentDisplay').html(`
        <div class="content-text p-4">
            ${content.replace(/\n/g, '<br>')}
        </div>
    `);
}

function displayImageContent(url) {
    $('#contentDisplay').html(`
        <div class="content-image text-center p-4">
            <img src="${url}" alt="Đề bài" class="img-fluid" style="max-height: 70vh;" />
        </div>
    `);
}

function displayPDFContent(url) {
    $('#contentDisplay').html(`
        <div class="content-pdf">
            <iframe src="${url}" width="100%" height="600px" frameborder="0"></iframe>
        </div>
    `);
}

function displayVideoContent(url) {
    $('#contentDisplay').html(`
        <div class="content-video p-4">
            <video controls width="100%" style="max-height: 70vh;">
                <source src="${url}" type="video/mp4">
                Trình duyệt không hỗ trợ video.
            </video>
        </div>
    `);
}

// ========================================
// SUBMIT TEST
// ========================================

function showSubmitConfirmation() {
    const unanswered = totalQuestions - answeredCount;
    let warning = '';
    
    if (unanswered > 0) {
        warning = `Bạn chưa trả lời ${unanswered}/${totalQuestions} câu hỏi. `;
    } else {
        warning = `Bạn đã trả lời đầy đủ ${totalQuestions} câu hỏi. `;
    }
    
    $('#submitWarning').text(warning);
    $('#submitModal').modal('show');
}

function submitTest() {
    isSubmitting = true;
    $('#btnConfirmSubmit').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý...');
    
    $.ajax({
        url: '/MockTest/SubmitTest',
        method: 'POST',
        data: { id: attemptId },
        success: function () {
            clearInterval(timerInterval);
            clearInterval(autoSaveInterval);
            window.location.href = `/MockTest/TestResult/${attemptId}`;
        },
        error: function () {
            isSubmitting = false;
            $('#btnConfirmSubmit').prop('disabled', false).text('Nộp bài');
            showError('Không thể nộp bài. Vui lòng thử lại.');
        }
    });
}

function autoSubmitTest() {
    isSubmitting = true;
    showWarning('Hết giờ!', 'Bài thi đang được tự động nộp...');
    
    $.ajax({
        url: '/MockTest/SubmitTest',
        method: 'POST',
        data: { id: attemptId },
        success: function () {
            window.location.href = `/MockTest/TestResult/${attemptId}`;
        },
        error: function () {
            showError('Không thể nộp bài tự động. Vui lòng reload trang.');
        }
    });
}

// ========================================
// SAVE DRAFT
// ========================================

function saveDraft() {
    const $btn = $('#btnSaveDraft');
    const $text = $('#saveText');
    
    $btn.prop('disabled', true);
    $text.html('<span class="spinner-border spinner-border-sm me-1"></span>Đang lưu...');
    
    // Just sync - actual saving happens on answer change
    setTimeout(() => {
        $btn.prop('disabled', false);
        $text.text('Đã lưu');
        
        setTimeout(() => {
            $text.text('Lưu');
        }, 2000);
    }, 500);
}

// ========================================
// UI HELPERS
// ========================================

function showSuccessIndicator() {
    // Brief green flash on save button
    const $btn = $('#btnSaveDraft');
    $btn.addClass('btn-success').removeClass('btn-primary');
    setTimeout(() => {
        $btn.addClass('btn-primary').removeClass('btn-success');
    }, 300);
}

function showWarning(title, message) {
    // You can implement a toast or alert here
    alert(title + '\n\n' + message);
}

function showError(message) {
    alert('Lỗi: ' + message);
}

function updateConnectionStatus() {
    // Check connection every 10 seconds
    setInterval(() => {
        if (navigator.onLine) {
            $('#connectionStatus').text('Đã kết nối');
        } else {
            $('#connectionStatus').text('Mất kết nối');
        }
    }, 10000);
}

function toggleFullscreen() {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen();
        $('#btnFullscreen i').removeClass('bi-arrows-fullscreen').addClass('bi-fullscreen-exit');
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
            $('#btnFullscreen i').removeClass('bi-fullscreen-exit').addClass('bi-arrows-fullscreen');
        }
    }
}
