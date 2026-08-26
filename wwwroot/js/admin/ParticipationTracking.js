function initializeMemberParticipation() {

    // =====================================================
    // 1. GET THE ELEMENTS FROM THE MEMBER MODAL
    // =====================================================

    const searchInput =
        document.querySelector('.js-member-search');

    const sortSelect =
        document.querySelector('.js-member-sort');

    const tbody =
        document.querySelector('.js-member-tbody');

    const container =
        document.querySelector('.member-participation-widget');


    // Stop if the modal/table has not been loaded yet
    if (!searchInput || !sortSelect || !tbody || !container) {
        console.log('Member participation elements not found.');
        return;
    }


    // =====================================================
    // 2. GET ALL MEMBER ROWS
    // =====================================================

    const allRows = Array.from(
        tbody.querySelectorAll('tr[data-member-name]')
    );


    // If there are no members, stop here
    if (allRows.length === 0) {
        return;
    }


    // =====================================================
    // 3. REMOVE OLD PAGINATION
    // =====================================================
    // This is useful when you close the modal and open
    // another group. It prevents duplicate pagination.

    const oldPagination =
        container.querySelector('.member-table-pagination');

    if (oldPagination) {
        oldPagination.remove();
    }


    // =====================================================
    // 4. CREATE PAGINATION AREA
    // =====================================================

    const paginationContainer =
        document.createElement('div');

    paginationContainer.className =
        'member-table-pagination';

    paginationContainer.innerHTML = `
        <div class="member-pagination-info js-pagination-info">
            Showing 0-0 of 0 members
        </div>

        <div class="member-pagination-list js-pagination-list">
        </div>
    `;


    container.appendChild(paginationContainer);


    const paginationInfo =
        paginationContainer.querySelector('.js-pagination-info');

    const paginationList =
        paginationContainer.querySelector('.js-pagination-list');


    // =====================================================
    // 5. PAGINATION SETTINGS
    // =====================================================

    const rowsPerPage = 5;

    let currentPage = 1;

    let filteredRows = [...allRows];


    // =====================================================
    // 6. UPDATE TABLE
    // =====================================================

    function updateTable() {

        // Get what the user typed
        const searchTerm =
            searchInput.value
                .toLowerCase()
                .trim();


        // Get selected sorting option
        const sortOrder =
            sortSelect.value;


        // =================================================
        // A. SEARCH
        // =================================================

        filteredRows = allRows.filter(row => {

            const memberName =
                (row.getAttribute('data-member-name') || '')
                    .toLowerCase();

            return memberName.includes(searchTerm);

        });


        // =================================================
        // B. SORT BY POINTS
        // =================================================

        filteredRows.sort((a, b) => {

            const pointsA =
                parseInt(
                    a.getAttribute('data-points') || '0',
                    10
                );


            const pointsB =
                parseInt(
                    b.getAttribute('data-points') || '0',
                    10
                );


            // Highest → Lowest
            if (sortOrder === 'highest') {
                return pointsB - pointsA;
            }


            // Lowest → Highest
            return pointsA - pointsB;

        });


        // =================================================
        // C. CLEAR CURRENT TABLE
        // =================================================

        tbody.innerHTML = '';


        // =================================================
        // D. NO SEARCH RESULTS
        // =================================================

        if (filteredRows.length === 0) {

            tbody.innerHTML = `
                <tr class="js-empty-row">
                    <td colspan="7"
                        class="text-center text-muted py-4">

                        No members found matching your search.

                    </td>
                </tr>
            `;


            paginationContainer.style.display = 'none';

            return;
        }


        // Show pagination again
        paginationContainer.style.display = 'flex';


        // =================================================
        // E. CALCULATE PAGINATION
        // =================================================

        const totalItems =
            filteredRows.length;


        const totalPages =
            Math.ceil(totalItems / rowsPerPage);


        // Protect against invalid page number
        if (currentPage > totalPages) {
            currentPage = totalPages;
        }


        if (currentPage < 1) {
            currentPage = 1;
        }


        const startIndex =
            (currentPage - 1) * rowsPerPage;


        const endIndex =
            Math.min(
                startIndex + rowsPerPage,
                totalItems
            );


        // =================================================
        // F. DISPLAY ONLY CURRENT PAGE
        // =================================================

        for (
            let i = startIndex;
            i < endIndex;
            i++
        ) {

            tbody.appendChild(
                filteredRows[i]
            );

        }


        // =================================================
        // G. UPDATE PAGINATION TEXT
        // =================================================

        paginationInfo.textContent =
            `Showing ${startIndex + 1}-${endIndex} of ${totalItems} members`;


        // =================================================
        // H. CREATE PAGE BUTTONS
        // =================================================

        renderPaginationControls(totalPages);
    }



    // =====================================================
    // 7. CREATE PAGINATION BUTTONS
    // =====================================================

    function renderPaginationControls(totalPages) {

        // Clear old buttons
        paginationList.innerHTML = '';


        // =================================================
        // PREVIOUS BUTTON
        // =================================================

        const prevBtn =
            document.createElement('button');


        prevBtn.type = 'button';


        prevBtn.innerHTML =
            '<i class="bi bi-chevron-left"></i>';


        prevBtn.disabled =
            currentPage === 1;


        prevBtn.addEventListener(
            'click',
            function () {

                if (currentPage > 1) {

                    currentPage--;

                    updateTable();

                }

            }
        );


        paginationList.appendChild(prevBtn);



        // =================================================
        // PAGE NUMBER BUTTONS
        // =================================================

        for (
            let i = 1;
            i <= totalPages;
            i++
        ) {

            const pageBtn =
                document.createElement('button');


            pageBtn.type = 'button';

            pageBtn.textContent = i;


            // Highlight current page
            if (i === currentPage) {
                pageBtn.classList.add('active');
            }


            pageBtn.addEventListener(
                'click',
                function () {

                    currentPage = i;

                    updateTable();

                }
            );


            paginationList.appendChild(pageBtn);
        }



        // =================================================
        // NEXT BUTTON
        // =================================================

        const nextBtn =
            document.createElement('button');


        nextBtn.type = 'button';


        nextBtn.innerHTML =
            '<i class="bi bi-chevron-right"></i>';


        nextBtn.disabled =
            currentPage === totalPages;


        nextBtn.addEventListener(
            'click',
            function () {

                if (currentPage < totalPages) {

                    currentPage++;

                    updateTable();

                }

            }
        );


        paginationList.appendChild(nextBtn);
    }



    // =====================================================
    // 8. SEARCH EVENT
    // =====================================================

    searchInput.addEventListener(
        'input',
        function () {

            // Whenever the user searches,
            // go back to page 1.
            currentPage = 1;

            updateTable();

        }
    );



    // =====================================================
    // 9. SORT EVENT
    // =====================================================

    sortSelect.addEventListener(
        'change',
        function () {

            // Whenever sorting changes,
            // go back to page 1.
            currentPage = 1;

            updateTable();

        }
    );



    // =====================================================
    // 10. INITIAL DISPLAY
    // =====================================================

    updateTable();
}