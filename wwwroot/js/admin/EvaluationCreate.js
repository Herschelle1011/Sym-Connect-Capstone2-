// evaluationCreate.js
// Handles the dynamic "Add Question" list on Evaluations/Create.cshtml
// Every question block's inputs must be named Questions[INDEX].Field
// so ASP.NET Core's model binder can rebuild the list on submit.

document.addEventListener('DOMContentLoaded', function () {
    const container = document.getElementById('questions-container');
    const template = document.getElementById('question-template');
    const addBtn = document.getElementById('add-question-btn');

    if (!container || !template || !addBtn) return;

    addBtn.addEventListener('click', addQuestionBlock);
    container.addEventListener('click', handleContainerClick);

    function addQuestionBlock() {
        const nextIndex = container.querySelectorAll('.question-block').length;

        // Clone the hidden <template> content
        const clone = template.content.cloneNode(true);

        // Swap placeholders for the real index/label before inserting into the page
        const html = clone.querySelector('.question-block').outerHTML
            .split('__INDEX__').join(nextIndex)
            .split('__LABEL__').join(nextIndex + 1);

        container.insertAdjacentHTML('beforeend', html);
    }

    function handleContainerClick(e) {
        const removeBtn = e.target.closest('.remove-question-btn');
        if (removeBtn) {
            removeBtn.closest('.question-block').remove();
            reindexQuestionBlocks();
            return;
        }

        const chip = e.target.closest('.suggestion-chip');
        if (chip) {
            const input = chip.closest('.question-block').querySelector('.question-text-input');
            if (input) input.value = chip.textContent.trim();
        }
    }

    // After removing a block, the remaining blocks' "Questions[X]" indexes
    // must be renumbered with no gaps, or the model binder will skip them.
    function reindexQuestionBlocks() {
        const blocks = container.querySelectorAll('.question-block');
        blocks.forEach((block, index) => {
            const label = block.querySelector('.question-header-row .form-label-sm');
            if (label) label.textContent = `Question ${index + 1}`;

            block.querySelectorAll('[name^="Questions["]').forEach((field) => {
                field.name = field.name.replace(/Questions\[\d+\]/, `Questions[${index}]`);
            });
        });
    }
});