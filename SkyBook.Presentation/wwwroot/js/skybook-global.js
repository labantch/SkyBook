(function () {
    // MVC routes (not .html files)
    var routes = {
        home: '/Home/Index',
        flights: '/Flight/Results',
        seats: '/Booking/SelectSeats',
        passenger: '/Passenger/Details',
        confirm: '/Booking/ConfirmPayment',
        bookings: '/Booking/MyBookings',
        about: '/Home/About',
        skyclub: '/Home/SkyClub',
        profile: '/Profile/Index',
        login: '/Account/Login',
        register: '/Account/Register'
    };

    var path = (window.location.pathname || '').toLowerCase();
    var flowSteps = [
        ['Flights', routes.flights],
        ['Seats', routes.seats],
        ['Passenger Details', routes.passenger],
        ['Confirm & Pay', routes.confirm]
    ];

    var flowIndex = -1;
    if (path.indexOf('/flight') >= 0 || path.indexOf('/results') >= 0) flowIndex = 0;
    else if (path.indexOf('/selectseats') >= 0 || path.indexOf('/seat') >= 0) flowIndex = 1;
    else if (path.indexOf('/passenger') >= 0 || path.indexOf('/details') >= 0) flowIndex = 2;
    else if (path.indexOf('/confirmpayment') >= 0 || path.indexOf('/confirm') >= 0) flowIndex = 3;

    var isAuthPage = path.indexOf('/account') >= 0 || path.indexOf('/login') >= 0 || path.indexOf('/register') >= 0;
    var rawName = (localStorage.getItem('skybook-user-name') || '').trim();
    var hasRealName = rawName.length > 0 && rawName.toLowerCase() !== 'guest';
    var loggedIn = !isAuthPage && localStorage.getItem('skybook-authenticated') === 'true' && hasRealName;

    if (!isAuthPage && localStorage.getItem('skybook-authenticated') === 'true' && !hasRealName) {
        localStorage.removeItem('skybook-authenticated');
        localStorage.removeItem('skybook-role');
        localStorage.removeItem('skybook-user-name');
        localStorage.removeItem('skybook-user-initials');
    }

    var userName = rawName;
    var userInitials = localStorage.getItem('skybook-user-initials') ||
        (userName.split(' ').map(function (p) { return p[0] || ''; }).join('').slice(0, 2).toUpperCase() || 'SB');

    var activeNav = '';
    if (path.indexOf('/mybookings') >= 0) activeNav = 'bookings';
    else if (path.indexOf('/profile') >= 0) activeNav = 'profile';
    else if (path.indexOf('/about') >= 0) activeNav = 'about';
    else if (path.indexOf('/home') >= 0 || path === '/' || path.indexOf('/flight') >= 0 || path.indexOf('/index') >= 0) activeNav = 'flights';

    function link(label, target, key) {
        return '<a href="' + target + '" data-skybook-nav="' + key + '"' +
            (activeNav === key ? ' class="skybook-active" aria-current="page"' : '') + '>' + label + '</a>';
    }

    var account = loggedIn
        ? '<a class="skybook-profile" href="' + routes.profile + '" aria-label="Open profile"><span class="skybook-avatar">' + userInitials + '</span><span><span class="skybook-profile-name">' + userName + '</span></span><span aria-hidden="true">⌄</span></a>'
        : '<a class="skybook-login" href="' + routes.login + '">Log In</a><a class="skybook-signup" href="' + routes.register + '">Sign Up</a>';

    var header = document.createElement('header');
    header.id = 'skybook-global-header';
    var profileHref = loggedIn ? routes.profile : routes.login;

    header.innerHTML =
        '<div class="skybook-header-inner">' +
        '<a class="skybook-brand" href="' + routes.home + '" aria-label="SkyBook home">' +
        '<svg class="skybook-brand-mark" viewBox="0 0 48 40" fill="none" aria-hidden="true">' +
        '<path d="M4 34 L40 4 L30 26 L14 30 Z" fill="#D4A017"/>' +
        '<path d="M14 30 L40 4 L34 36 L22 32 Z" fill="#0B192C"/>' +
        '<path d="M14 30 L30 26 L22 32 Z" fill="#E8C547"/>' +
        '</svg>' +
        '<span class="skybook-wordmark">' +
        '<span class="skybook-name">SKY<span>BOOK</span></span>' +
        '<span class="skybook-tagline">Luxury Airways</span>' +
        '</span>' +
        '</a>' +
        '<button class="skybook-menu" type="button" aria-label="Toggle navigation">☰</button>' +
        '<nav class="skybook-nav" aria-label="Primary navigation">' +
        link('Flights', routes.home, 'flights') +
        link('My Bookings', routes.bookings, 'bookings') +
        link('About', routes.about, 'about') +
        link('Profile', profileHref, 'profile') +
        '</nav>' +
        '<div class="skybook-account">' + account + '</div>' +
        '</div>';

    document.body.insertBefore(header, document.body.firstChild);
    header.querySelector('.skybook-menu').addEventListener('click', function () {
        header.classList.toggle('skybook-open');
    });

    if (flowIndex >= 0) {
        var progress = document.createElement('nav');
        progress.id = 'skybook-booking-progress';
        progress.setAttribute('aria-label', 'Booking progress');
        progress.innerHTML = '<div class="skybook-progress-inner">' + flowSteps.map(function (step, index) {
            return (index ? '<span class="skybook-separator" aria-hidden="true">›</span>' : '') +
                '<a class="skybook-step' + (index === flowIndex ? ' skybook-current' : '') + '" href="' + step[1] + '">' +
                '<span class="skybook-step-number">' + (index + 1) + '</span><span>' + step[0].toUpperCase() + '</span></a>';
        }).join('') + '</div>';
        header.insertAdjacentElement('afterend', progress);
    }
})();