// =========================
// DELEGATED EVENT LISTENERS
// (works even when the form is injected later into #appModal)
// =========================

document.addEventListener('change', function (e) {
    if (e.target && e.target.id === 'provinceSelect') {
        handleProvinceChange(e.target);
    }

    if (e.target && e.target.id === 'citySelect') {
        handleCityChange(e.target);
    }
});


// =========================
// MODAL OPEN
// =========================
document.addEventListener('shown.bs.modal', function (e) {
    const provinceSelect = document.getElementById('provinceSelect');

    if (provinceSelect) {
        initializeAddress(provinceSelect);
    }
});


// If your Create/Edit form ever opens in an offcanvas instead of a modal,
// keep this too — it's a no-op otherwise.
document.addEventListener('shown.bs.offcanvas', function (e) {
    const provinceSelect = document.getElementById('provinceSelect');

    if (provinceSelect) {
        initializeAddress(provinceSelect);
    }
});


// =========================
// INITIALIZE ADDRESS
// Handles both Create and Edit
// =========================
async function initializeAddress(provinceSelect) {

    const savedProvince =
        document.getElementById('savedProvince')?.value;

    const savedCity =
        document.getElementById('savedCity')?.value;

    const savedBarangay =
        document.getElementById('savedBarangay')?.value;

    // Load provinces first
    await loadProvinces(provinceSelect);

    // CREATE FORM:
    // No saved province, so stop here.
    if (!savedProvince) {
        return;
    }

    // EDIT FORM:
    // Find the saved province by its displayed name.
    const provinceOption = Array.from(provinceSelect.options)
        .find(option =>
            option.textContent.trim().toLowerCase()
            === savedProvince.trim().toLowerCase()
        );

    if (!provinceOption) {
        console.warn('Saved province not found:', savedProvince);
        return;
    }

    // Select the saved province
    provinceSelect.value = provinceOption.value;

    const provinceName = document.getElementById('provinceName');

    if (provinceName) {
        provinceName.value = savedProvince;
    }

    // Now load and restore city
    await loadCitiesForEdit(
        provinceSelect.value,
        savedCity,
        savedBarangay
    );
}


// =========================
// LOAD PROVINCES
// =========================
function loadProvinces(provinceSelect) {

    return fetch('https://psgc.gitlab.io/api/provinces/')
        .then(response => response.json())
        .then(provinces => {

            provinceSelect.innerHTML =
                '<option value="">-- Select Province --</option>';

            provinces
                .sort((a, b) => a.name.localeCompare(b.name))
                .forEach(province => {

                    const option = document.createElement('option');

                    option.value = province.code;
                    option.textContent = province.name;

                    provinceSelect.appendChild(option);
                });
        })
        .catch(error => console.error('Province error:', error));
}


// =========================
// LOAD CITIES FOR EDIT
// =========================
function loadCitiesForEdit(provinceCode, savedCity, savedBarangay) {

    const citySelect = document.getElementById('citySelect');
    const barangaySelect = document.getElementById('barangaySelect');
    const cityName = document.getElementById('cityName');

    citySelect.innerHTML =
        '<option value="">-- Select City/Municipality --</option>';

    barangaySelect.innerHTML =
        '<option value="">-- Select City First --</option>';

    citySelect.disabled = true;
    barangaySelect.disabled = true;

    return fetch(
        `https://psgc.gitlab.io/api/provinces/${provinceCode}/cities-municipalities/`
    )
        .then(response => response.json())
        .then(cities => {

            cities
                .sort((a, b) => a.name.localeCompare(b.name))
                .forEach(city => {

                    const option = document.createElement('option');

                    option.value = city.code;
                    option.textContent = city.name;

                    citySelect.appendChild(option);
                });

            citySelect.disabled = false;

            if (!savedCity) {
                return;
            }

            const cityOption = Array.from(citySelect.options)
                .find(option =>
                    option.textContent.trim().toLowerCase()
                    === savedCity.trim().toLowerCase()
                );

            if (!cityOption) {
                console.warn('Saved city not found:', savedCity);
                return;
            }

            citySelect.value = cityOption.value;

            if (cityName) {
                cityName.value = savedCity;
            }

            return loadBarangaysForEdit(
                citySelect.value,
                savedBarangay
            );
        })
        .catch(error => console.error('City error:', error));
}


// =========================
// LOAD BARANGAYS FOR EDIT
// =========================
function loadBarangaysForEdit(cityCode, savedBarangay) {

    const barangaySelect = document.getElementById('barangaySelect');

    barangaySelect.innerHTML =
        '<option value="">-- Select Barangay --</option>';

    barangaySelect.disabled = true;

    return fetch(
        `https://psgc.gitlab.io/api/cities-municipalities/${cityCode}/barangays/`
    )
        .then(response => response.json())
        .then(barangays => {

            barangays
                .sort((a, b) => a.name.localeCompare(b.name))
                .forEach(barangay => {

                    const option = document.createElement('option');

                    option.value = barangay.name;
                    option.textContent = barangay.name;

                    barangaySelect.appendChild(option);
                });

            barangaySelect.disabled = false;

            if (!savedBarangay) {
                return;
            }

            const barangayOption =
                Array.from(barangaySelect.options)
                    .find(option =>
                        option.textContent.trim().toLowerCase()
                        === savedBarangay.trim().toLowerCase()
                    );

            if (barangayOption) {
                barangaySelect.value = barangayOption.value;
            }
            else {
                console.warn(
                    'Saved barangay not found:',
                    savedBarangay
                );
            }
        })
        .catch(error =>
            console.error('Barangay error:', error)
        );
}


// =========================
// PROVINCE CHANGE
// =========================
function handleProvinceChange(provinceSelect) {

    const citySelect = document.getElementById('citySelect');
    const barangaySelect = document.getElementById('barangaySelect');
    const provinceName = document.getElementById('provinceName');
    const cityName = document.getElementById('cityName');

    citySelect.innerHTML =
        '<option value="">-- Select City/Municipality --</option>';

    barangaySelect.innerHTML =
        '<option value="">-- Select City First --</option>';

    citySelect.disabled = true;
    barangaySelect.disabled = true;

    cityName.value = '';

    if (!provinceSelect.value) {
        provinceName.value = '';
        return;
    }

    const selectedOption =
        provinceSelect.options[provinceSelect.selectedIndex];

    // Store the NAME (not the code) in the hidden input that actually gets posted
    provinceName.value = selectedOption.textContent.trim();

    fetch(
        `https://psgc.gitlab.io/api/provinces/${provinceSelect.value}/cities-municipalities/`
    )
        .then(response => response.json())
        .then(cities => {

            cities
                .sort((a, b) => a.name.localeCompare(b.name))
                .forEach(city => {

                    const option = document.createElement('option');

                    option.value = city.code;
                    option.textContent = city.name;

                    citySelect.appendChild(option);
                });

            citySelect.disabled = false;
        })
        .catch(error => console.error('City error:', error));
}


// =========================
// CITY CHANGE
// =========================
function handleCityChange(citySelect) {

    const barangaySelect = document.getElementById('barangaySelect');
    const cityName = document.getElementById('cityName');

    barangaySelect.innerHTML =
        '<option value="">-- Select Barangay --</option>';

    barangaySelect.disabled = true;

    if (!citySelect.value) {
        cityName.value = '';
        return;
    }

    const selectedOption =
        citySelect.options[citySelect.selectedIndex];

    // Store the NAME (not the code) in the hidden input that actually gets posted
    cityName.value = selectedOption.textContent.trim();

    fetch(
        `https://psgc.gitlab.io/api/cities-municipalities/${citySelect.value}/barangays/`
    )
        .then(response => response.json())
        .then(barangays => {

            barangays
                .sort((a, b) => a.name.localeCompare(b.name))
                .forEach(barangay => {

                    const option = document.createElement('option');

                    option.value = barangay.name;
                    option.textContent = barangay.name;

                    barangaySelect.appendChild(option);
                });

            barangaySelect.disabled = false;
        })
        .catch(error =>
            console.error('Barangay error:', error)
        );
}