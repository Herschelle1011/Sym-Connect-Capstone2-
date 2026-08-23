function filterMembers(status, btn) {
    document.querySelectorAll('.tab-btn')
        .forEach(b => b.classList.remove('active'));
    btn.classList.add('active');

    const rows = document.querySelectorAll('#membersBody tr:not(#emptyRow)');

    rows.forEach(row => {
        row.style.display =
            (status === 'all' || row.dataset.status === status)
                ? '' : 'none';
    });
}


    let selectedMembers = [];
// was: const groupId = @Model.GroupId;
const groupId = document.querySelector('.container-fluid').dataset.groupId;

//OPEN ADD MEMBER MODAL
function openAddMemberModal() {
    selectedMembers = [];

    document.getElementById('memberSearchInput').value = '';

    document.getElementById('searchResults').innerHTML =
        '<div style="padding:16px;text-align:center;color:#9ca3af;font-size:13px;">Loading members...</div>';

    renderTags();
    updateConfirmBtn();

    document.getElementById('addMemberModal').style.display = 'flex';

    fetchMembers('');
}

//CLOSE MEMBER MODAL
function closeAddMemberModal() {
    document.getElementById('addMemberModal').style.display = 'none';
}

// Debounce — waits 300ms after user stops typing before calling server
// Think of it like waiting for someone to finish their sentence before replying
let searchTimer;

function searchMembers(query) {
    clearTimeout(searchTimer);

    searchTimer = setTimeout(() => fetchMembers(query), 300);
}

// Calls your SearchMembers controller action
function fetchMembers(query) {
    fetch(`/Groups/SearchMembers?query=${encodeURIComponent(query)}&groupId=${groupId}`)
        .then(r => r.json())
        .then(members => renderResults(members));
}

function renderResults(members) {
    const container = document.getElementById('searchResults');

    if (members.length === 0) {
        container.innerHTML =
            '<div style="padding:16px;text-align:center;color:#9ca3af;font-size:13px;">No members found.</div>';
        return;
    }

    container.innerHTML = members.map(m => {

        const initials = m.fullName
            .split(' ')
            .filter(x => x.length > 0)
            .slice(0, 2)
            .map(x => x[0])
            .join('')
            .toUpperCase();
        const isSelected = selectedMembers.some(s => s.id === m.memberId);

        const checkIcon = isSelected
            ? '<i class="ti ti-circle-check" style="color:#16a34a;font-size:18px;"></i>'
            : '<i class="ti ti-circle" style="color:#d1d5db;font-size:18px;"></i>';

        return `
            <div onclick="toggleMember(
                    ${m.memberId},
                    '${m.fullName.replace(/'/g, "\\'")}'
                 )"
                 style="
                    display:flex;
                    align-items:center;
                    gap:10px;
                    padding:10px 14px;
                    cursor:pointer;
                    border-bottom:1px solid #f3f4f6;
                    background:${isSelected ? '#f0fdf4' : '#fff'};
                 ">

                <div style="
                    width:36px;
                    height:36px;
                    border-radius:50%;
                    background:#dbeafe;
                    color:#1e40af;
                    display:flex;
                    align-items:center;
                    justify-content:center;
                    font-size:12px;
                    font-weight:500;
                    flex-shrink:0;
                ">
                    ${initials}
                </div>

                <div style="flex:1; min-width:0;">

                    <div style="
                        font-size:13px;
                        font-weight:500;
                        color:#111827;
                    ">
                        ${m.fullName}
                    </div>

                    <div style="
                        font-size:11px;
                        color:#6b7280;
                        overflow:hidden;
                        text-overflow:ellipsis;
                        white-space:nowrap;
                    ">
                        ${m.email}
                    </div>

                </div>

                <div style="
                    display:flex;
                    align-items:center;
                    gap:10px;
                    flex-shrink:0;
                ">

                    <span style="
                        background:#f0fdf4;
                        color:#15803d;
                        padding:4px 8px;
                        border-radius:999px;
                        font-size:11px;
                        font-weight:600;
                        white-space:nowrap;
                    ">
                        ${m.points} pts
                    </span>

                    ${checkIcon}

                </div>
            </div>
        `;
    }).join('');
}

// Toggle: if already selected → remove, if not → add
function toggleMember(id, fullName) {
    const exists = selectedMembers.findIndex(m => m.id === id);
    if (exists >= 0) {
        selectedMembers.splice(exists, 1); // remove
    } else {
        selectedMembers.push({ id, fullName }); // add
    }
    renderTags();
    updateConfirmBtn();
    // Re-render results to update checkmarks
    fetchMembers(document.getElementById('memberSearchInput').value);
}

// Shows the purple tag chips at the top
function renderTags() {
    const container = document.getElementById('selectedTags');
    container.innerHTML = selectedMembers.map(m => `
                <span style="display:inline-flex;align-items:center;gap:4px;
                             background:#ede9fe;color:#5b21b6;
                             border-radius:99px;padding:4px 10px;font-size:12px;font-weight:500;">
                    ${m.fullName}
                    <i class="ti ti-x" onclick="toggleMember(${m.id}, '${m.fullName.replace(/'/g, "\\'")}')"
                       style="cursor:pointer;font-size:11px;"></i>
                </span>`).join('');

    // Build hidden inputs so the form submits all selected 



    document.getElementById('hiddenMemberInputs').innerHTML =
        selectedMembers.map(m =>
            `<input type="hidden" name="memberIds" value="${m.id}" />`
        ).join('');
}

// Updates the confirm button text + enabled state
function updateConfirmBtn() {
    const btn = document.getElementById('confirmAddBtn');
    const count = selectedMembers.length;
    btn.textContent = count === 0
        ? 'Add 0 members to group'
        : `Add ${count} member${count > 1 ? 's' : ''} to group`;
    btn.disabled = count === 0;
    btn.style.opacity = count === 0 ? '0.5' : '1';
    btn.style.cursor = count === 0 ? 'default' : 'pointer';
}

// Close modal if user clicks the dark background
document.getElementById('addMemberModal')
    .addEventListener('click', function (e) {
        if (e.target === this) closeAddMemberModal();
    });





document.addEventListener('submit', function (e) {
    if (e.target.id !== 'removeMemberForm') return;
    e.preventDefault();

    const form = e.target;
    const formData = new FormData(form);
    const token = formData.get('__RequestVerificationToken');

    fetch('/Groups/RemoveMemberFromGroup', {
        method: 'POST',
        headers: { 'RequestVerificationToken': token },
        body: formData
    })
        .then(r => r.json())
        .then(result => {
            if (!result.success) return;

            const memberId = formData.get('memberId');
            document.querySelector(`tr[data-member-id="${memberId}"]`)?.remove();

            bootstrap.Modal.getInstance(document.getElementById('appModal'))?.hide();
        });
});


document.addEventListener('submit', function (e) {
    if (e.target.id !== 'transferMemberForm') return;
    e.preventDefault();

    const form = e.target;
    const formData = new FormData(form);
    const token = formData.get('__RequestVerificationToken');

    fetch('/Groups/ConfirmedTranferMember', {
        method: 'POST',
        headers: { 'RequestVerificationToken': token },
        body: formData
    })
        .then(r => r.json())
        .then(result => {
            if (!result.success) return;

            const memberId = formData.get('memberId');
            document.querySelector(`tr[data-member-id="${memberId}"]`)?.remove();

            bootstrap.Modal.getInstance(document.getElementById('appModal'))?.hide();
        });
});