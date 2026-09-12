console.log("membership-registration.js loaded");

let currentRegistrationStep = 1;
let emailVerified = false;
let registrationProfilePhotoData = null;

const totalRegistrationSteps = 5;


/* =========================================================
   GET IMPORTANT ELEMENTS
========================================================= */

const registrationModal =
    document.getElementById("membershipRegistrationModal");

const registrationForm =
    document.getElementById("membershipRegistrationForm");

const nextButton =
    document.getElementById("registrationNextBtn");

const backButton =
    document.getElementById("registrationBackBtn");

const submitButton =
    document.getElementById("registrationSubmitBtn");

const sendCodeButton =
    document.getElementById("sendVerificationCodeBtn");

const verifyCodeButton =
    document.getElementById("verifyCodeBtn");


/* =========================================================
   AUTO-CALCULATE AGE
========================================================= */

const dobInput =
    document.getElementById("registrationDob");

const ageInput =
    document.getElementById("registrationAge");


function calculateAge(dobValue) {

    if (!dobValue) {
        return "";
    }

    const dob = new Date(dobValue);
    const today = new Date();

    let age =
        today.getFullYear() -
        dob.getFullYear();

    const monthDiff =
        today.getMonth() -
        dob.getMonth();

    if (
        monthDiff < 0 ||
        (
            monthDiff === 0 &&
            today.getDate() < dob.getDate()
        )
    ) {
        age--;
    }

    return age >= 0
        ? age
        : "";
}


dobInput?.addEventListener(
    "change",
    function () {

        if (ageInput) {
            ageInput.value =
                calculateAge(this.value);
        }
    }
);


/* =========================================================
   PASSWORD VISIBILITY
========================================================= */

document
    .querySelectorAll(".toggle-password")
    .forEach(function (button) {

        button.addEventListener(
            "click",
            function () {

                const targetId =
                    this.dataset.target;

                const input =
                    document.getElementById(
                        targetId
                    );

                const icon =
                    this.querySelector("i");

                if (!input) {
                    return;
                }

                const isHidden =
                    input.type === "password";

                input.type =
                    isHidden
                        ? "text"
                        : "password";

                if (icon) {

                    icon.classList.toggle(
                        "bi-eye"
                    );

                    icon.classList.toggle(
                        "bi-eye-slash"
                    );
                }
            }
        );
    });


/* =========================================================
   SHOW REGISTRATION STEP
========================================================= */

function showRegistrationStep(step) {

    const sections =
        document.querySelectorAll(
            ".registration-section"
        );

    const indicators =
        document.querySelectorAll(
            ".registration-step"
        );


    sections.forEach(section => {

        const sectionStep =
            Number(
                section.dataset.step
            );

        section.style.display =
            sectionStep === step
                ? "block"
                : "none";
    });


    indicators.forEach(indicator => {

        const indicatorStep =
            Number(
                indicator.dataset
                    .stepIndicator
            );

        indicator.classList.remove(
            "active",
            "completed"
        );


        if (indicatorStep === step) {

            indicator.classList.add(
                "active"
            );

        }
        else if (
            indicatorStep < step
        ) {

            indicator.classList.add(
                "completed"
            );
        }
    });


    if (backButton) {

        backButton.style.display =
            step === 1
                ? "none"
                : "inline-flex";
    }


    if (nextButton) {

        nextButton.style.display =
            step === totalRegistrationSteps
                ? "none"
                : "inline-flex";
    }


    if (submitButton) {

        submitButton.style.display =
            step === totalRegistrationSteps
                ? "inline-flex"
                : "none";
    }


    if (step === 2) {

        updateVerificationEmailPreview();
    }


    if (step === 5) {

        populateReview();
    }


    currentRegistrationStep = step;


    const modalBody =
        document.querySelector(
            "#membershipRegistrationModal .modal-body"
        );

    if (modalBody) {
        modalBody.scrollTop = 0;
    }
}


/* =========================================================
   INITIALIZE STEP 1
========================================================= */

document.addEventListener(
    "DOMContentLoaded",
    function () {

        showRegistrationStep(1);
    }
);


/* =========================================================
   EMAIL PREVIEW
========================================================= */

function updateVerificationEmailPreview() {

    const email =
        document.getElementById(
            "registrationEmail"
        )?.value.trim();

    const preview =
        document.getElementById(
            "verificationEmailPreview"
        );

    if (preview) {

        preview.textContent =
            email ||
            "your email address";
    }
}


document.addEventListener(
    "input",
    function (event) {

        if (
            event.target.id !==
            "registrationEmail"
        ) {
            return;
        }

        updateVerificationEmailPreview();
    }
);


/* =========================================================
   LOAD REGISTRATION GROUPS
========================================================= */

async function loadRegistrationGroups() {

    const groupSelect =
        document.getElementById(
            "registrationGroupId"
        );

    if (!groupSelect) {

        console.error(
            "Registration group dropdown not found."
        );

        return;
    }


    try {

        groupSelect.disabled = true;

        groupSelect.innerHTML =
            '<option value="">Loading groups...</option>';


        const response =
            await fetch(
                "/Identity/Account/Register?handler=Groups"
            );


        if (!response.ok) {

            throw new Error(
                "Unable to load groups."
            );
        }


        const groups =
            await response.json();


        console.log(
            "Registration groups:",
            groups
        );


        groupSelect.innerHTML =
            '<option value="">Select your preferred group</option>';


        groups.forEach(group => {

            const option =
                document.createElement(
                    "option"
                );

            option.value =
                group.groupId;

            option.textContent =
                group.groupName;

            groupSelect.appendChild(
                option
            );
        });


        groupSelect.disabled = false;

    }
    catch (error) {

        console.error(
            "Error loading groups:",
            error
        );

        groupSelect.innerHTML =
            '<option value="">Unable to load groups</option>';

        groupSelect.disabled = true;
    }
}


/* =========================================================
   MODAL OPEN
========================================================= */

registrationModal?.addEventListener(
    "show.bs.modal",
    function () {

        console.log(
            "Registration modal opened"
        );

        loadRegistrationGroups();

        currentRegistrationStep = 1;

        showRegistrationStep(1);
    }
);


/* =========================================================
   VALIDATION HELPERS
========================================================= */

function clearFieldError(element) {

    if (
        !element ||
        !element.classList
    ) {
        return;
    }


    element.classList.remove(
        "is-invalid"
    );


    const wrapper =
        element.closest(
            ".field-icon-wrap"
        );


    const container =
        wrapper || element;


    const parent =
        container.parentElement;


    if (!parent) {
        return;
    }


    const existingError =
        parent.querySelector(
            ".registration-field-error"
        );


    if (existingError) {

        existingError.remove();
    }
}


function showFieldError(
    element,
    message
) {

    if (
        !element ||
        !element.classList
    ) {
        return;
    }


    clearFieldError(element);


    element.classList.add(
        "is-invalid"
    );


    const wrapper =
        element.closest(
            ".field-icon-wrap"
        );


    const container =
        wrapper || element;


    const error =
        document.createElement(
            "div"
        );


    error.className =
        "registration-field-error";


    error.textContent =
        message;


    container.insertAdjacentElement(
        "afterend",
        error
    );
}


/* =========================================================
   PH MOBILE NUMBER CHECK
   Accepts: 09XXXXXXXXX, +639XXXXXXXXX, 639XXXXXXXXX
   (spaces / dashes / parentheses are stripped first)
========================================================= */

function isValidPhMobileNumber(value) {

    if (!value) {
        return false;
    }

    const cleaned =
        value.replace(/[\s\-()]/g, "");

    return /^(?:\+63\d{10}|63\d{10}|09\d{9})$/.test(
        cleaned
    );
}


/* =========================================================
   PASSWORD STRENGTH CHECK
   At least 8 characters, with a letter, a number, and a symbol
   (matches the helper text shown under the Password field)
========================================================= */

function isStrongPassword(value) {

    return /^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9\s]).{8,}$/.test(
        value || ""
    );
}


/* =========================================================
   EMAIL / USERNAME AVAILABILITY CHECKS
========================================================= */

function getAntiForgeryToken() {

    return registrationForm
        ?.querySelector(
            'input[name="__RequestVerificationToken"]'
        )
        ?.value;
}


async function checkEmailAvailability(email) {

    const token = getAntiForgeryToken();

    try {

        const response =
            await fetch(
                "/Identity/Account/Register?handler=CheckEmail",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token
                    },
                    body: JSON.stringify({ email })
                }
            );

        return await response.json();

    }
    catch (error) {

        console.error(
            "Error checking email availability:",
            error
        );

        // Fail open on network errors — the server re-checks on final submit anyway.
        return { available: true };
    }
}


async function checkUsernameAvailability(username) {

    const token = getAntiForgeryToken();

    try {

        const response =
            await fetch(
                "/Identity/Account/Register?handler=CheckUsername",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token
                    },
                    body: JSON.stringify({ username })
                }
            );

        return await response.json();

    }
    catch (error) {

        console.error(
            "Error checking username availability:",
            error
        );

        return { available: true };
    }
}


/* =========================================================
   STEP 1 VALIDATION
========================================================= */

function validateStep1() {

    let valid = true;


    const firstName =
        document.querySelector(
            '[name="FirstName"]'
        );

    const lastName =
        document.querySelector(
            '[name="LastName"]'
        );

    const birthDate =
        document.getElementById(
            "registrationDob"
        );

    const age =
        document.getElementById(
            "registrationAge"
        );

    const gender =
        document.querySelector(
            '[name="Gender"]'
        );

    const contact =
        document.querySelector(
            '[name="ContactNumber"]'
        );

    const province =
        document.getElementById(
            "provinceSelect"
        );

    const city =
        document.getElementById(
            "citySelect"
        );

    const barangay =
        document.getElementById(
            "barangaySelect"
        );

    const group =
        document.getElementById(
            "registrationGroupId"
        );


    [
        firstName,
        lastName,
        birthDate,
        age,
        gender,
        contact,
        province,
        city,
        barangay,
        group
    ].forEach(
        clearFieldError
    );


    if (!firstName?.value.trim()) {

        showFieldError(
            firstName,
            "First name is required."
        );

        valid = false;
    }


    if (!lastName?.value.trim()) {

        showFieldError(
            lastName,
            "Last name is required."
        );

        valid = false;
    }


    if (!birthDate?.value) {

        showFieldError(
            birthDate,
            "Date of birth is required."
        );

        valid = false;
    }


    if (!age?.value) {

        showFieldError(
            age,
            "Please select your date of birth."
        );

        valid = false;
    }


    if (!gender?.value) {

        showFieldError(
            gender,
            "Gender is required."
        );

        valid = false;
    }


    if (!contact?.value.trim()) {

        showFieldError(
            contact,
            "Contact number is required."
        );

        valid = false;
    }
    else if (!isValidPhMobileNumber(contact.value.trim())) {

        showFieldError(
            contact,
            "Enter a valid PH mobile number (e.g. 09XXXXXXXXX or +639XXXXXXXXX)."
        );

        valid = false;
    }


    if (!province?.value) {

        showFieldError(
            province,
            "Province is required."
        );

        valid = false;
    }


    if (!city?.value) {

        showFieldError(
            city,
            "City / Municipality is required."
        );

        valid = false;
    }


    if (!barangay?.value) {

        showFieldError(
            barangay,
            "Barangay is required."
        );

        valid = false;
    }


    if (!group?.value) {

        showFieldError(
            group,
            "Group preference is required."
        );

        valid = false;
    }


    return valid;
}


/* =========================================================
   STEP 2 VALIDATION (async — checks email/username availability)
========================================================= */

async function validateStep2() {

    let valid = true;


    const email =
        document.getElementById(
            "registrationEmail"
        );

    const username =
        document.querySelector(
            '[name="Username"]'
        );

    const password =
        document.getElementById(
            "registrationPassword"
        );

    const confirmPassword =
        document.getElementById(
            "registrationConfirmPassword"
        );


    [
        email,
        username,
        password,
        confirmPassword
    ].forEach(
        clearFieldError
    );


    if (!email?.value.trim()) {

        showFieldError(
            email,
            "Email address is required."
        );

        valid = false;
    }
    else if (
        !email.checkValidity()
    ) {

        showFieldError(
            email,
            "Please enter a valid email address."
        );

        valid = false;
    }


    if (!username?.value.trim()) {

        showFieldError(
            username,
            "Username is required."
        );

        valid = false;
    }


    if (!password?.value) {

        showFieldError(
            password,
            "Password is required."
        );

        valid = false;
    }
    else if (!isStrongPassword(password.value)) {

        showFieldError(
            password,
            "Use at least 8 characters with a letter, a number, and a symbol."
        );

        valid = false;
    }


    if (!confirmPassword?.value) {

        showFieldError(
            confirmPassword,
            "Please confirm your password."
        );

        valid = false;
    }


    if (
        password?.value &&
        confirmPassword?.value &&
        password.value !==
        confirmPassword.value
    ) {

        showFieldError(
            confirmPassword,
            "Passwords do not match."
        );

        valid = false;
    }


    if (!valid) {

        showAlertDialog(
            "Please complete all required fields correctly before continuing.",
            "error",
            "Missing or Invalid Information"
        );

        return false;
    }


    // Availability checks only run once the fields above are already valid,
    // so we don't fire requests for obviously-incomplete input.

    const emailCheck =
        await checkEmailAvailability(
            email.value.trim()
        );

    if (!emailCheck.available) {

        showFieldError(
            email,
            emailCheck.message || "This email address is already registered."
        );

        showAlertDialog(
            emailCheck.message || "This email address is already registered.",
            "error",
            "Email Unavailable"
        );

        return false;
    }


    const usernameCheck =
        await checkUsernameAvailability(
            username.value.trim()
        );

    if (!usernameCheck.available) {

        showFieldError(
            username,
            usernameCheck.message || "This username is already taken."
        );

        showAlertDialog(
            usernameCheck.message || "This username is already taken.",
            "error",
            "Username Unavailable"
        );

        return false;
    }


    if (!emailVerified) {

        showAlertDialog(
            "Please verify your email before continuing.",
            "error",
            "Email Not Verified"
        );

        return false;
    }


    return true;
}


/* =========================================================
   CONTINUE BUTTON
========================================================= */

nextButton?.addEventListener(
    "click",
    async function () {

        nextButton.disabled = true;


        try {

            if (
                currentRegistrationStep === 1
            ) {

                if (!validateStep1()) {

                    showAlertDialog(
                        "Please complete all required fields correctly before continuing.",
                        "error",
                        "Missing or Invalid Information"
                    );

                    return;
                }
            }


            if (
                currentRegistrationStep === 2
            ) {

                const stepTwoValid =
                    await validateStep2();

                if (!stepTwoValid) {
                    return;
                }
            }


            if (
                currentRegistrationStep === 3
            ) {

                const guidelines =
                    document.querySelector(
                        '[name="AcceptedGuidelines"]'
                    );


                if (!guidelines?.checked) {

                    showAlertDialog(
                        "Please read and accept the Membership Guidelines.",
                        "error",
                        "Agreement Required"
                    );

                    return;
                }
            }


            if (
                currentRegistrationStep === 4
            ) {

                const privacy =
                    document.querySelector(
                        '[name="AcceptedPrivacy"]'
                    );


                if (!privacy?.checked) {

                    showAlertDialog(
                        "Please read and accept the Privacy Notice.",
                        "error",
                        "Agreement Required"
                    );

                    return;
                }
            }


            if (
                currentRegistrationStep <
                totalRegistrationSteps
            ) {

                showRegistrationStep(
                    currentRegistrationStep + 1
                );
            }

        }
        finally {

            nextButton.disabled = false;
        }
    }
);


/* =========================================================
   BACK BUTTON
========================================================= */

backButton?.addEventListener(
    "click",
    function () {

        if (
            currentRegistrationStep > 1
        ) {

            showRegistrationStep(
                currentRegistrationStep - 1
            );
        }
    }
);


/* =========================================================
   SEND EMAIL VERIFICATION CODE
========================================================= */

sendCodeButton?.addEventListener(
    "click",
    async function () {

        const emailInput =
            document.getElementById(
                "registrationEmail"
            );


        const email =
            emailInput?.value.trim();


        if (!email) {

            showAlertDialog(
                "Please enter your email address first.",
                "error",
                "Email Required"
            );

            return;
        }


        if (
            emailInput &&
            !emailInput.checkValidity()
        ) {

            showAlertDialog(
                "Please enter a valid email address.",
                "error",
                "Invalid Email"
            );

            return;
        }


        const token =
            registrationForm
                ?.querySelector(
                    'input[name="__RequestVerificationToken"]'
                )
                ?.value;


        if (!token) {

            console.error(
                "Anti-forgery token was not found."
            );

            return;
        }


        try {

            sendCodeButton.disabled =
                true;

            sendCodeButton.innerHTML =
                'Sending...';


            const response =
                await fetch(
                    "/Identity/Account/Register?handler=SendCode",
                    {
                        method: "POST",

                        headers: {

                            "Content-Type":
                                "application/json",

                            "RequestVerificationToken":
                                token
                        },

                        body:
                            JSON.stringify({
                                email: email
                            })
                    }
                );


            const result =
                await response.json();


            showAlertDialog(
                result.message,
                result.success ? "success" : "error",
                result.success ? "Code Sent" : "Unable to Send Code"
            );


            if (result.success) {

                const codeContainer =
                    document.getElementById(
                        "verificationCodeContainer"
                    );


                if (codeContainer) {

                    codeContainer.style.display =
                        "block";
                }
            }

        }
        catch (error) {

            console.error(
                "Error sending verification code:",
                error
            );


            showAlertDialog(
                "Something went wrong while sending the verification code.",
                "error",
                "Something Went Wrong"
            );

        }
        finally {

            if (!emailVerified) {

                sendCodeButton.disabled =
                    false;

                sendCodeButton.innerHTML =
                    '<i class="bi bi-send me-2"></i>Send Verification Code';
            }
        }
    }
);


/* =========================================================
   VERIFY EMAIL CODE
========================================================= */

verifyCodeButton?.addEventListener(
    "click",
    async function () {

        const email =
            document.getElementById(
                "registrationEmail"
            )
                ?.value
                ?.trim();


        const code =
            document.getElementById(
                "verificationCode"
            )
                ?.value
                ?.trim();


        if (!email) {

            showAlertDialog(
                "Please enter your email address.",
                "error",
                "Email Required"
            );

            return;
        }


        if (!code) {

            showAlertDialog(
                "Please enter the verification code.",
                "error",
                "Code Required"
            );

            return;
        }


        const token =
            registrationForm
                ?.querySelector(
                    'input[name="__RequestVerificationToken"]'
                )
                ?.value;


        if (!token) {

            console.error(
                "Anti-forgery token was not found."
            );

            return;
        }


        try {

            verifyCodeButton.disabled =
                true;

            verifyCodeButton.textContent =
                "Verifying...";


            const response =
                await fetch(
                    "/Identity/Account/Register?handler=VerifyCode",
                    {
                        method: "POST",

                        headers: {

                            "Content-Type":
                                "application/json",

                            "RequestVerificationToken":
                                token
                        },

                        body:
                            JSON.stringify({
                                email: email,
                                code: code
                            })
                    }
                );


            const result =
                await response.json();


            if (result.success) {

                emailVerified = true;


                const successMessage =
                    document.getElementById(
                        "emailVerifiedMessage"
                    );


                if (successMessage) {

                    successMessage.style.display =
                        "flex";
                }


                const emailInput =
                    document.getElementById(
                        "registrationEmail"
                    );


                const codeInput =
                    document.getElementById(
                        "verificationCode"
                    );


                if (emailInput) {

                    emailInput.readOnly =
                        true;
                }


                if (codeInput) {

                    codeInput.readOnly =
                        true;
                }


                verifyCodeButton.disabled =
                    true;

                verifyCodeButton.textContent =
                    "Verified";


                if (sendCodeButton) {

                    sendCodeButton.disabled =
                        true;
                }


                showAlertDialog(
                    result.message,
                    "success",
                    "Email Verified"
                );

            }
            else {

                showAlertDialog(
                    result.message,
                    "error",
                    "Verification Failed"
                );
            }

        }
        catch (error) {

            console.error(
                "Error verifying email:",
                error
            );


            showAlertDialog(
                "Something went wrong while verifying your email.",
                "error",
                "Something Went Wrong"
            );

        }
        finally {

            if (!emailVerified) {

                verifyCodeButton.disabled =
                    false;

                verifyCodeButton.textContent =
                    "Verify Code";
            }
        }
    }
);


/* =========================================================
   OPTIONAL PROFILE PHOTO
========================================================= */

const profilePhotoInput =
    document.getElementById(
        "registrationProfilePhoto"
    );

const uploadPhotoButton =
    document.getElementById(
        "registrationUploadPhotoBtn"
    );

const photoCameraButton =
    document.getElementById(
        "registrationPhotoButton"
    );

const removePhotoButton =
    document.getElementById(
        "registrationRemovePhotoBtn"
    );

const profilePreview =
    document.getElementById(
        "registrationProfilePreview"
    );

const profilePlaceholder =
    document.getElementById(
        "registrationProfilePlaceholder"
    );


uploadPhotoButton?.addEventListener(
    "click",
    function () {

        profilePhotoInput?.click();
    }
);


photoCameraButton?.addEventListener(
    "click",
    function () {

        profilePhotoInput?.click();
    }
);


profilePhotoInput?.addEventListener(
    "change",
    function () {

        const file =
            this.files?.[0];


        if (!file) {
            return;
        }


        const allowedTypes = [
            "image/jpeg",
            "image/png"
        ];


        if (
            !allowedTypes.includes(
                file.type
            )
        ) {

            showAlertDialog(
                "Please select a JPG or PNG image.",
                "error",
                "Invalid File Type"
            );

            this.value = "";

            return;
        }


        const maxSize =
            2 * 1024 * 1024;


        if (
            file.size > maxSize
        ) {

            showAlertDialog(
                "Profile photo must not be larger than 2 MB.",
                "error",
                "File Too Large"
            );

            this.value = "";

            return;
        }


        const reader =
            new FileReader();


        reader.onload =
            function (event) {

                registrationProfilePhotoData =
                    event.target.result;


                if (profilePreview) {

                    profilePreview.src =
                        registrationProfilePhotoData;

                    profilePreview.style.display =
                        "block";
                }


                if (profilePlaceholder) {

                    profilePlaceholder.style.display =
                        "none";
                }


                if (removePhotoButton) {

                    removePhotoButton.style.display =
                        "inline-block";
                }
            };


        reader.readAsDataURL(
            file
        );
    }
);


removePhotoButton?.addEventListener(
    "click",
    function () {

        if (profilePhotoInput) {

            profilePhotoInput.value =
                "";
        }


        registrationProfilePhotoData =
            null;


        if (profilePreview) {

            profilePreview.src =
                "";

            profilePreview.style.display =
                "none";
        }


        if (profilePlaceholder) {

            profilePlaceholder.style.display =
                "flex";
        }


        removePhotoButton.style.display =
            "none";
    }
);


/* =========================================================
   POPULATE STEP 5 REVIEW
========================================================= */

function populateReview() {

    const firstName =
        document.querySelector(
            '[name="FirstName"]'
        )?.value ?? "";


    const middleName =
        document.querySelector(
            '[name="MiddleName"]'
        )?.value ?? "";


    const lastName =
        document.querySelector(
            '[name="LastName"]'
        )?.value ?? "";


    const email =
        document.querySelector(
            '[name="Email"]'
        )?.value ?? "";


    const contact =
        document.querySelector(
            '[name="ContactNumber"]'
        )?.value ?? "";


    const gender =
        document.querySelector(
            '[name="Gender"]'
        )?.value ?? "";


    const birthDate =
        document.querySelector(
            '[name="DateOfBirth"]'
        )?.value ?? "";


    const age =
        document.getElementById(
            "registrationAge"
        )?.value ?? "";


    const country =
        document.querySelector(
            '[name="Country"]'
        )?.value ?? "";


    const province =
        document.getElementById(
            "provinceName"
        )?.value ?? "";


    const city =
        document.getElementById(
            "cityName"
        )?.value ?? "";


    const barangay =
        document.getElementById(
            "barangaySelect"
        )?.value ?? "";


    const address =
        [
            barangay,
            city,
            province,
            country
        ]
            .filter(value => value)
            .join(", ");


    const username =
        document.querySelector(
            '[name="Username"]'
        )?.value ?? "";


    const groupSelect =
        document.getElementById(
            "registrationGroupId"
        );


    let groupName =
        "-";


    if (
        groupSelect &&
        groupSelect.selectedIndex > 0
    ) {

        groupName =
            groupSelect.options[
                groupSelect.selectedIndex
            ].text;
    }


    const fullName =
        `${firstName} ${middleName} ${lastName}`
            .replace(
                /\s+/g,
                " "
            )
            .trim();


    setReviewText(
        "reviewName",
        fullName
    );


    setReviewText(
        "reviewEmail",
        email
    );


    setReviewText(
        "reviewContact",
        contact
    );


    setReviewText(
        "reviewGender",
        gender
    );


    setReviewText(
        "reviewBirthDate",
        formatReviewDate(
            birthDate
        )
    );


    setReviewText(
        "reviewAge",
        age
    );


    setReviewText(
        "reviewAddress",
        address
    );


    setReviewText(
        "reviewUsername",
        username
    );


    setReviewText(
        "reviewGroup",
        groupName
    );


    const reviewProfilePhoto =
        document.getElementById(
            "reviewProfilePhoto"
        );


    const reviewProfilePlaceholder =
        document.getElementById(
            "reviewProfilePlaceholder"
        );


    if (
        registrationProfilePhotoData
    ) {

        if (reviewProfilePhoto) {

            reviewProfilePhoto.src =
                registrationProfilePhotoData;

            reviewProfilePhoto.style.display =
                "block";
        }


        if (reviewProfilePlaceholder) {

            reviewProfilePlaceholder.style.display =
                "none";
        }

    }
    else {

        if (reviewProfilePhoto) {

            reviewProfilePhoto.src =
                "";

            reviewProfilePhoto.style.display =
                "none";
        }


        if (reviewProfilePlaceholder) {

            reviewProfilePlaceholder.style.display =
                "flex";
        }
    }
}


/* =========================================================
   FORMAT REVIEW DATE
========================================================= */

function formatReviewDate(
    value
) {

    if (!value) {
        return "-";
    }


    const date =
        new Date(
            value + "T00:00:00"
        );


    return date.toLocaleDateString(
        "en-US",
        {
            month: "long",
            day: "numeric",
            year: "numeric"
        }
    );
}


/* =========================================================
   REVIEW TEXT HELPER
========================================================= */

function setReviewText(
    elementId,
    value
) {

    const element =
        document.getElementById(
            elementId
        );


    if (element) {

        element.textContent =
            value || "-";
    }
}


/* =========================================================
   SUBMIT WHOLE REGISTRATION
========================================================= */

registrationForm?.addEventListener(
    "submit",
    async function (event) {

        event.preventDefault();


        if (!emailVerified) {

            showAlertDialog(
                "Please verify your email before submitting.",
                "error",
                "Email Not Verified"
            );

            return;
        }


        const acceptedGuidelines =
            document.querySelector(
                '[name="AcceptedGuidelines"]'
            );


        if (
            !acceptedGuidelines?.checked
        ) {

            showAlertDialog(
                "Please accept the Membership Guidelines.",
                "error",
                "Agreement Required"
            );

            return;
        }


        const acceptedPrivacy =
            document.querySelector(
                '[name="AcceptedPrivacy"]'
            );


        if (
            !acceptedPrivacy?.checked
        ) {

            showAlertDialog(
                "Please accept the Privacy Notice.",
                "error",
                "Agreement Required"
            );

            return;
        }


        try {

            submitButton.disabled =
                true;

            submitButton.textContent =
                "Submitting...";


            const formData =
                new FormData(
                    registrationForm
                );


            const response =
                await fetch(
                    "/Identity/Account/Register",
                    {
                        method: "POST",
                        body: formData
                    }
                );


            const result =
                await response.json();


            if (result.success) {

                showAlertDialog(
                    result.message,
                    "success",
                    "Application Submitted"
                );


                const modal =
                    bootstrap.Modal
                        .getInstance(
                            registrationModal
                        );


                modal?.hide();


                registrationForm.reset();


                resetRegistrationForm();

            }
            else {

                showAlertDialog(
                    result.message,
                    "error",
                    "Submission Failed"
                );
            }

        }
        catch (error) {

            console.error(
                "Registration submission error:",
                error
            );


            showAlertDialog(
                "Something went wrong while submitting your application.",
                "error",
                "Something Went Wrong"
            );

        }
        finally {

            submitButton.disabled =
                false;

            submitButton.textContent =
                "Submit Application";
        }
    }
);


/* =========================================================
   RESET REGISTRATION
========================================================= */

function resetRegistrationForm() {

    currentRegistrationStep =
        1;

    emailVerified =
        false;

    registrationProfilePhotoData =
        null;


    const emailInput =
        document.getElementById(
            "registrationEmail"
        );

    const codeInput =
        document.getElementById(
            "verificationCode"
        );

    const codeContainer =
        document.getElementById(
            "verificationCodeContainer"
        );

    const verifiedMessage =
        document.getElementById(
            "emailVerifiedMessage"
        );


    if (emailInput) {

        emailInput.readOnly =
            false;
    }


    if (codeInput) {

        codeInput.value =
            "";

        codeInput.readOnly =
            false;
    }


    if (codeContainer) {

        codeContainer.style.display =
            "none";
    }


    if (verifiedMessage) {

        verifiedMessage.style.display =
            "none";
    }


    if (sendCodeButton) {

        sendCodeButton.disabled =
            false;

        sendCodeButton.innerHTML =
            '<i class="bi bi-send me-2"></i>Send Verification Code';
    }


    if (verifyCodeButton) {

        verifyCodeButton.disabled =
            false;

        verifyCodeButton.textContent =
            "Verify Code";
    }


    if (profilePhotoInput) {

        profilePhotoInput.value =
            "";
    }


    if (profilePreview) {

        profilePreview.src =
            "";

        profilePreview.style.display =
            "none";
    }


    if (profilePlaceholder) {

        profilePlaceholder.style.display =
            "flex";
    }


    if (removePhotoButton) {

        removePhotoButton.style.display =
            "none";
    }


    if (ageInput) {

        ageInput.value =
            "";
    }


    document
        .querySelectorAll(
            ".registration-field-error"
        )
        .forEach(error =>
            error.remove()
        );


    document
        .querySelectorAll(
            ".is-invalid"
        )
        .forEach(element =>
            element.classList.remove(
                "is-invalid"
            )
        );


    showRegistrationStep(1);
}