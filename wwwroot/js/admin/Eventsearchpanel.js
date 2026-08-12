document.addEventListener('DOMContentLoaded', function () {
    const searchPanel = document.getElementById('searchEventPanel');
    const searchInput = document.getElementById('eventSearchInput');
    const resultsContainer = document.getElementById('eventSearchResults');
    const cardTemplate = document.getElementById('eventResultCardTemplate');

    let debounceTimer = null;

    // Load the full unfiltered list every time the panel opens
    searchPanel.addEventListener('shown.bs.offcanvas', function () {
        searchInput.value = '';
        fetchEvents('');
    });

    searchInput.addEventListener('input', function () {
        const query = searchInput.value.trim();

        clearTimeout(debounceTimer);

        debounceTimer = setTimeout(function () {
            fetchEvents(query);   // fetch even when query is empty, so clearing the box restores the full list
        }, 300);
    });

    async function fetchEvents(query) {
        try {
            const response = await fetch(`/Events/SearchPanel?search=${encodeURIComponent(query)}`);

            if (!response.ok) {
                console.error('Search request failed:', response.status);
                return;
            }

            const events = await response.json();
            renderResults(events);
        } catch (err) {
            console.error('Error fetching search results:', err);
        }
    }

    function renderResults(events) {
        resultsContainer.innerHTML = '';

        if (!events || events.length === 0) {
            resultsContainer.innerHTML = '<div class="text-muted small p-2">No events found.</div>';
            return;
        }

        events.forEach(function (ev) {
            const card = cardTemplate.content.cloneNode(true);

            const link = card.querySelector('.event-result-card');
            link.href = `/Events/Details/${ev.eventId}`;

            const startDate = new Date(ev.startDate);
            const day = startDate.getDate();
            const month = startDate.toLocaleString('en-US', { month: 'short' }).toUpperCase();

            card.querySelector('.event-day').textContent = day;
            card.querySelector('.event-month').textContent = month;
            card.querySelector('.event-title').textContent = ev.eventTitle;
            card.querySelector('.event-facility').textContent = ev.facilityName;

            const timeEl = card.querySelector('.event-time');
            if (ev.isAllDay) {
                timeEl.textContent = 'All day';
            } else if (ev.startTime && ev.endTime) {
                timeEl.textContent = `${formatTime(ev.startTime)} - ${formatTime(ev.endTime)}`;
            } else {
                timeEl.textContent = '';
            }

            resultsContainer.appendChild(card);
        });
    }

    function formatTime(timeSpanString) {
        const [hoursStr, minutesStr] = timeSpanString.split(':');
        let hours = parseInt(hoursStr, 10);
        const minutes = minutesStr;
        const period = hours >= 12 ? 'PM' : 'AM';
        hours = hours % 12 || 12;
        return `${hours}:${minutes} ${period}`;
    }
});

// 1. How many elements actually have this id?
document.querySelectorAll('#eventSearchInput').length