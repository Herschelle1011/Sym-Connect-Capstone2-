document.addEventListener('DOMContentLoaded', function () {
    const provinceSelect = document.getElementById('provinceSelect');
    const citySelect = document.getElementById('citySelect');
    const barangaySelect = document.getElementById('barangaySelect');

    if (!provinceSelect) return; // not on this page

    // Load provinces on page load
    fetch('https://psgc.gitlab.io/api/provinces/')
        .then(res => res.json())
        .then(provinces => {
            provinces
                .sort((a, b) => a.name.localeCompare(b.name))
                .forEach(p => {
                    const opt = document.createElement('option');
                    opt.value = p.code;
                    opt.textContent = p.name;
                    provinceSelect.appendChild(opt);
                });
        })
        .catch(err => console.error('Failed to load provinces:', err));

    // When province changes, load its cities/municipalities
    provinceSelect.addEventListener('change', function () {
        citySelect.innerHTML = '<option value="">-- Select City/Municipality --</option>';
        barangaySelect.innerHTML = '<option value="">-- Select City First --</option>';
        citySelect.disabled = true;
        barangaySelect.disabled = true;

        if (!this.value) return;

        fetch(`https://psgc.gitlab.io/api/provinces/${this.value}/cities-municipalities/`)
            .then(res => res.json())
            .then(cities => {
                cities
                    .sort((a, b) => a.name.localeCompare(b.name))
                    .forEach(c => {
                        const opt = document.createElement('option');
                        opt.value = c.code;
                        opt.textContent = c.name;
                        citySelect.appendChild(opt);
                    });
                citySelect.disabled = false;
            })
            .catch(err => console.error('Failed to load cities:', err));
    });

    // When city changes, load its barangays
    citySelect.addEventListener('change', function () {
        barangaySelect.innerHTML = '<option value="">-- Select Barangay --</option>';
        barangaySelect.disabled = true;

        if (!this.value) return;

        fetch(`https://psgc.gitlab.io/api/cities-municipalities/${this.value}/barangays/`)
            .then(res => res.json())
            .then(barangays => {
                barangays
                    .sort((a, b) => a.name.localeCompare(b.name))
                    .forEach(b => {
                        const opt = document.createElement('option');
                        opt.value = b.name; // barangays don't need to carry a code further down
                        opt.textContent = b.name;
                        barangaySelect.appendChild(opt);
                    });
                barangaySelect.disabled = false;
            })
            .catch(err => console.error('Failed to load barangays:', err));
    });
});