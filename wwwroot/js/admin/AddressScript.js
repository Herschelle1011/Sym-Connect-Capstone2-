// =========================
// ADDRESS SELECTOR
// Default:
// Country = Philippines
// Province = Cebu
// =========================


// =========================
// DELEGATED EVENT LISTENERS
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

document.addEventListener('shown.bs.modal', function () {

    const provinceSelect =
        document.getElementById('provinceSelect');

    if (provinceSelect) {
        initializeAddress(provinceSelect);
    }

});


// =========================
// OFFCANVAS OPEN
// Keep this if you also use
// the form inside an offcanvas.
// =========================

document.addEventListener('shown.bs.offcanvas', function () {

    const provinceSelect =
        document.getElementById('provinceSelect');

    if (provinceSelect) {
        initializeAddress(provinceSelect);
    }

});


// =========================
// INITIALIZE ADDRESS
// Defaults to Cebu
// Also supports Edit forms
// =========================

async function initializeAddress(provinceSelect) {

    const savedProvince =
        document.getElementById('savedProvince')?.value
        || 'Cebu';

    const savedCity =
        document.getElementById('savedCity')?.value
        || '';

    const savedBarangay =
        document.getElementById('savedBarangay')?.value
        || '';

    const provinceName =
        document.getElementById('provinceName');


    // Load all Philippine provinces first
    await loadProvinces(provinceSelect);


    // Find Cebu or saved province
    const provinceOption =
        Array.from(provinceSelect.options)
            .find(option =>
                option.textContent
                    .trim()
                    .toLowerCase()
                ===
                savedProvince
                    .trim()
                    .toLowerCase()
            );


    if (!provinceOption) {

        console.warn(
            'Saved province not found:',
            savedProvince
        );

        return;
    }


    // Select province
    provinceSelect.value =
        provinceOption.value;


    // Store province NAME
    if (provinceName) {

        provinceName.value =
            provinceOption.textContent.trim();
    }


    // Load cities for selected province
    await loadCitiesForEdit(
        provinceSelect.value,
        savedCity,
        savedBarangay
    );
}


// =========================
// LOAD PROVINCES
// =========================

async function loadProvinces(provinceSelect) {

    try {

        provinceSelect.disabled = true;

        provinceSelect.innerHTML =
            '<option value="">Loading provinces...</option>';


        const response =
            await fetch(
                'https://psgc.gitlab.io/api/provinces/'
            );


        if (!response.ok) {

            throw new Error(
                'Unable to load provinces.'
            );
        }


        const provinces =
            await response.json();


        provinceSelect.innerHTML =
            '<option value="">-- Select Province --</option>';


        provinces
            .sort(
                (a, b) =>
                    a.name.localeCompare(b.name)
            )
            .forEach(province => {

                const option =
                    document.createElement('option');

                option.value =
                    province.code;

                option.textContent =
                    province.name;

                provinceSelect.appendChild(
                    option
                );
            });


        provinceSelect.disabled = false;

    }
    catch (error) {

        console.error(
            'Province error:',
            error
        );


        provinceSelect.innerHTML =
            '<option value="">Unable to load provinces</option>';

        provinceSelect.disabled = true;
    }
}


// =========================
// LOAD CITIES
// Also restores saved city
// and saved barangay if editing
// =========================

async function loadCitiesForEdit(
    provinceCode,
    savedCity,
    savedBarangay
) {

    const citySelect =
        document.getElementById('citySelect');

    const barangaySelect =
        document.getElementById('barangaySelect');

    const cityName =
        document.getElementById('cityName');


    if (!citySelect || !barangaySelect) {
        return;
    }


    citySelect.innerHTML =
        '<option value="">Loading cities...</option>';

    barangaySelect.innerHTML =
        '<option value="">-- Select City First --</option>';


    citySelect.disabled = true;

    barangaySelect.disabled = true;


    if (cityName) {
        cityName.value = '';
    }


    if (!provinceCode) {

        citySelect.innerHTML =
            '<option value="">-- Select City/Municipality --</option>';

        return;
    }


    try {

        const response =
            await fetch(
                `https://psgc.gitlab.io/api/provinces/${provinceCode}/cities-municipalities/`
            );


        if (!response.ok) {

            throw new Error(
                'Unable to load cities.'
            );
        }


        const cities =
            await response.json();


        citySelect.innerHTML =
            '<option value="">-- Select City/Municipality --</option>';


        cities
            .sort(
                (a, b) =>
                    a.name.localeCompare(b.name)
            )
            .forEach(city => {

                const option =
                    document.createElement('option');

                option.value =
                    city.code;

                option.textContent =
                    city.name;

                citySelect.appendChild(
                    option
                );
            });


        citySelect.disabled = false;


        // Create form:
        // Cebu is selected,
        // but no city is selected yet.
        if (!savedCity) {
            return;
        }


        // Edit form:
        // Find saved city by name.
        const cityOption =
            Array.from(citySelect.options)
                .find(option =>
                    option.textContent
                        .trim()
                        .toLowerCase()
                    ===
                    savedCity
                        .trim()
                        .toLowerCase()
                );


        if (!cityOption) {

            console.warn(
                'Saved city not found:',
                savedCity
            );

            return;
        }


        citySelect.value =
            cityOption.value;


        if (cityName) {

            cityName.value =
                cityOption.textContent.trim();
        }


        await loadBarangaysForEdit(
            citySelect.value,
            savedBarangay
        );

    }
    catch (error) {

        console.error(
            'City error:',
            error
        );


        citySelect.innerHTML =
            '<option value="">Unable to load cities</option>';

        citySelect.disabled = true;
    }
}


// =========================
// LOAD BARANGAYS
// Also restores saved barangay
// =========================

async function loadBarangaysForEdit(
    cityCode,
    savedBarangay
) {

    const barangaySelect =
        document.getElementById(
            'barangaySelect'
        );


    if (!barangaySelect) {
        return;
    }


    barangaySelect.innerHTML =
        '<option value="">Loading barangays...</option>';

    barangaySelect.disabled = true;


    if (!cityCode) {

        barangaySelect.innerHTML =
            '<option value="">-- Select Barangay --</option>';

        return;
    }


    try {

        const response =
            await fetch(
                `https://psgc.gitlab.io/api/cities-municipalities/${cityCode}/barangays/`
            );


        if (!response.ok) {

            throw new Error(
                'Unable to load barangays.'
            );
        }


        const barangays =
            await response.json();


        barangaySelect.innerHTML =
            '<option value="">-- Select Barangay --</option>';


        barangays
            .sort(
                (a, b) =>
                    a.name.localeCompare(b.name)
            )
            .forEach(barangay => {

                const option =
                    document.createElement('option');

                // We want the NAME posted,
                // not the PSGC code.
                option.value =
                    barangay.name;

                option.textContent =
                    barangay.name;

                barangaySelect.appendChild(
                    option
                );
            });


        barangaySelect.disabled = false;


        if (!savedBarangay) {
            return;
        }


        const barangayOption =
            Array.from(
                barangaySelect.options
            )
                .find(option =>
                    option.textContent
                        .trim()
                        .toLowerCase()
                    ===
                    savedBarangay
                        .trim()
                        .toLowerCase()
                );


        if (barangayOption) {

            barangaySelect.value =
                barangayOption.value;

        }
        else {

            console.warn(
                'Saved barangay not found:',
                savedBarangay
            );
        }

    }
    catch (error) {

        console.error(
            'Barangay error:',
            error
        );


        barangaySelect.innerHTML =
            '<option value="">Unable to load barangays</option>';

        barangaySelect.disabled = true;
    }
}


// =========================
// PROVINCE CHANGE
// =========================

async function handleProvinceChange(
    provinceSelect
) {

    const citySelect =
        document.getElementById('citySelect');

    const barangaySelect =
        document.getElementById(
            'barangaySelect'
        );

    const provinceName =
        document.getElementById(
            'provinceName'
        );

    const cityName =
        document.getElementById(
            'cityName'
        );


    if (!citySelect || !barangaySelect) {
        return;
    }


    // Reset city
    citySelect.innerHTML =
        '<option value="">-- Select City/Municipality --</option>';

    citySelect.disabled = true;


    // Reset barangay
    barangaySelect.innerHTML =
        '<option value="">-- Select City First --</option>';

    barangaySelect.disabled = true;


    // Reset hidden city name
    if (cityName) {
        cityName.value = '';
    }


    // No province selected
    if (!provinceSelect.value) {

        if (provinceName) {
            provinceName.value = '';
        }

        return;
    }


    const selectedOption =
        provinceSelect.options[
        provinceSelect.selectedIndex
        ];


    // Store province NAME instead of code
    if (provinceName) {

        provinceName.value =
            selectedOption.textContent.trim();
    }


    try {

        citySelect.innerHTML =
            '<option value="">Loading cities...</option>';


        const response =
            await fetch(
                `https://psgc.gitlab.io/api/provinces/${provinceSelect.value}/cities-municipalities/`
            );


        if (!response.ok) {

            throw new Error(
                'Unable to load cities.'
            );
        }


        const cities =
            await response.json();


        citySelect.innerHTML =
            '<option value="">-- Select City/Municipality --</option>';


        cities
            .sort(
                (a, b) =>
                    a.name.localeCompare(b.name)
            )
            .forEach(city => {

                const option =
                    document.createElement('option');

                option.value =
                    city.code;

                option.textContent =
                    city.name;

                citySelect.appendChild(
                    option
                );
            });


        citySelect.disabled = false;

    }
    catch (error) {

        console.error(
            'City error:',
            error
        );


        citySelect.innerHTML =
            '<option value="">Unable to load cities</option>';

        citySelect.disabled = true;
    }
}


// =========================
// CITY CHANGE
// =========================

async function handleCityChange(
    citySelect
) {

    const barangaySelect =
        document.getElementById(
            'barangaySelect'
        );

    const cityName =
        document.getElementById(
            'cityName'
        );


    if (!barangaySelect) {
        return;
    }


    // Reset barangay whenever city changes
    barangaySelect.innerHTML =
        '<option value="">-- Select Barangay --</option>';

    barangaySelect.disabled = true;


    // No city selected
    if (!citySelect.value) {

        if (cityName) {
            cityName.value = '';
        }

        return;
    }


    const selectedOption =
        citySelect.options[
        citySelect.selectedIndex
        ];


    // Store city NAME instead of PSGC code
    if (cityName) {

        cityName.value =
            selectedOption.textContent.trim();
    }


    try {

        barangaySelect.innerHTML =
            '<option value="">Loading barangays...</option>';


        const response =
            await fetch(
                `https://psgc.gitlab.io/api/cities-municipalities/${citySelect.value}/barangays/`
            );


        if (!response.ok) {

            throw new Error(
                'Unable to load barangays.'
            );
        }


        const barangays =
            await response.json();


        barangaySelect.innerHTML =
            '<option value="">-- Select Barangay --</option>';


        barangays
            .sort(
                (a, b) =>
                    a.name.localeCompare(b.name)
            )
            .forEach(barangay => {

                const option =
                    document.createElement('option');

                // Post the barangay NAME
                option.value =
                    barangay.name;

                option.textContent =
                    barangay.name;

                barangaySelect.appendChild(
                    option
                );
            });


        barangaySelect.disabled = false;

    }
    catch (error) {

        console.error(
            'Barangay error:',
            error
        );


        barangaySelect.innerHTML =
            '<option value="">Unable to load barangays</option>';

        barangaySelect.disabled = true;
    }
}