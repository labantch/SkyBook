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
        register: '/Account/Register',
        logout: '/Account/Logout'
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

    var isAuthPage = path.indexOf('/account/login') >= 0 || path.indexOf('/account/register') >= 0;

    // Synchronize server authentication state from body data attributes
    var body = document.body;
    var serverAuthed = body && body.getAttribute('data-authenticated') === 'true';
    var serverName = body && (body.getAttribute('data-user-name') || '').trim();
    var serverEmail = body && (body.getAttribute('data-user-email') || '').trim();
    var serverRole = body && (body.getAttribute('data-user-role') || 'user').trim();
    var serverInitials = body && (body.getAttribute('data-user-initials') || '').trim();
    var serverImage = body && (body.getAttribute('data-user-image') || '').trim();

    if (serverAuthed) {
        var finalName = serverName || 'Member';
        localStorage.setItem('skybook-authenticated', 'true');
        localStorage.setItem('skybook-user-name', finalName);
        if (serverEmail) localStorage.setItem('skybook-user-email', serverEmail);
        if (serverRole) localStorage.setItem('skybook-role', serverRole);
        if (serverInitials) localStorage.setItem('skybook-user-initials', serverInitials);
        if (serverImage) localStorage.setItem('skybook-user-avatar', serverImage);
        var authUser = {
            name: finalName,
            email: serverEmail,
            role: serverRole === 'admin' ? 'Admin' : 'Passenger',
            initials: serverInitials || 'SB',
            avatar: serverImage
        };
        localStorage.setItem('skybook-auth-user', JSON.stringify(authUser));
    } else if (body && body.hasAttribute('data-authenticated') && !serverAuthed) {
        // Server confirms not authenticated -> clear stale localStorage
        localStorage.removeItem('skybook-authenticated');
        localStorage.removeItem('skybook-role');
        localStorage.removeItem('skybook-user-name');
        localStorage.removeItem('skybook-user-email');
        localStorage.removeItem('skybook-user-initials');
        localStorage.removeItem('skybook-user-avatar');
        localStorage.removeItem('skybook-auth-user');
    }

    var rawName = (serverAuthed ? serverName : (localStorage.getItem('skybook-user-name') || '')).trim();
    var hasRealName = rawName.length > 0 && rawName.toLowerCase() !== 'guest';
    var loggedIn = !isAuthPage && (serverAuthed || (localStorage.getItem('skybook-authenticated') === 'true' && hasRealName));

    var userName = rawName || 'Member';
    var userInitials = (serverAuthed && serverInitials) ? serverInitials : (localStorage.getItem('skybook-user-initials') ||
        (userName.split(' ').map(function (p) { return p[0] || ''; }).join('').slice(0, 2).toUpperCase() || 'SB'));
    var userAvatar = (serverAuthed && serverImage) ? serverImage : localStorage.getItem('skybook-user-avatar');

    var activeNav = '';
    if (path.indexOf('/mybookings') >= 0) activeNav = 'bookings';
    else if (path.indexOf('/profile') >= 0) activeNav = 'profile';
    else if (path.indexOf('/admin') >= 0) activeNav = 'console';
    else if (path.indexOf('/about') >= 0) activeNav = 'about';
    else if (path.indexOf('/home') >= 0 || path === '/' || path.indexOf('/flight') >= 0 || path.indexOf('/index') >= 0) activeNav = 'flights';

    function link(label, target, key) {
        return '<a href="' + target + '" data-skybook-nav="' + key + '"' +
            (activeNav === key ? ' class="skybook-active" aria-current="page"' : '') + '>' + label + '</a>';
    }

    var avatarMarkup = userAvatar
        ? '<img src="' + userAvatar + '" alt="Avatar" style="width:100%;height:100%;object-fit:cover;border-radius:50%;" onerror="this.parentElement.textContent=\'' + userInitials + '\'">'
        : userInitials;

    var isAdmin = serverRole.toLowerCase() === 'admin' ||
                  (localStorage.getItem('skybook-role') || '').toLowerCase() === 'admin' ||
                  (serverEmail.toLowerCase() === 'skybook@gmail.com') ||
                  ((localStorage.getItem('skybook-user-email') || '').toLowerCase() === 'skybook@gmail.com');

    var adminConsoleBtn = isAdmin
        ? '<a href="/Admin/AdminDashboard/Index" id="skybook-admin-console-btn" title="Admin Dashboard Console" ' +
          'style="display:inline-flex;align-items:center;gap:6px;font-size:12px;font-weight:700;' +
          'color:#0B192C;background:linear-gradient(135deg,#D4A017,#E8C547);' +
          'border:none;border-radius:6px;padding:6px 12px;text-decoration:none;' +
          'box-shadow:0 2px 8px rgba(212,160,23,0.35);transition:transform 0.15s ease, opacity 0.15s ease;" ' +
          'onmouseover="this.style.opacity=\'0.9\';this.style.transform=\'translateY(-1px)\'" onmouseout="this.style.opacity=\'1\';this.style.transform=\'translateY(0)\'">' +
          '<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' +
          '<rect x="2" y="3" width="20" height="14" rx="2"/><path d="M8 21h8M12 17v4"/>' +
          '</svg>' +
          'Console</a>'
        : '';

    var divider = '<span style="height:28px;width:1px;background:#e2e8f0;display:inline-block;margin-right:2px;" aria-hidden="true"></span>';

    var account = loggedIn
        ? '<div style="display:inline-flex;align-items:center;gap:12px;">' +
          divider +
          adminConsoleBtn +
          '<a class="skybook-profile" style="border-left:none;padding-left:0;" href="' + routes.profile + '" aria-label="Open profile"><span class="skybook-avatar" style="overflow:hidden;padding:0;display:flex;align-items:center;justify-content:center;">' + avatarMarkup + '</span><span><span class="skybook-profile-name">' + userName + '</span></span></a>' +
          '<a class="skybook-logout-link" href="' + routes.logout + '" title="Sign Out" style="font-size:12px;font-weight:600;color:#e11d48;background:#fff1f2;border:1px solid #ffe4e6;border-radius:6px;padding:6px 10px;text-decoration:none;transition:background 0.15s ease;">Sign Out</a>' +
          '</div>'
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

    function isFlightsCompleted() {
        try {
            var raw = sessionStorage.getItem('skybook-selected-flights');
            if (!raw) return false;
            var data = JSON.parse(raw);
            if (!data || !Array.isArray(data.segments) || data.segments.length === 0) return false;
            return data.segments.every(function (s) {
                return s && s.selectedFlight && s.selectedFlight.flightNum;
            });
        } catch (e) {
            return false;
        }
    }

    function isSeatsCompleted() {
        if (!isFlightsCompleted()) return false;
        try {
            var raw = sessionStorage.getItem('skybook-seat-assignments');
            if (!raw) return false;
            var data = JSON.parse(raw);
            if (!data) return false;
            var keys = Object.keys(data);
            if (keys.length === 0) return false;
            return keys.every(function (k) {
                var seg = data[k];
                if (!seg || !seg.passengers) return false;
                var paxKeys = Object.keys(seg.passengers);
                return paxKeys.length > 0 && paxKeys.every(function (pk) {
                    return seg.passengers[pk] && seg.passengers[pk].seat;
                });
            });
        } catch (e) {
            return false;
        }
    }

    function isPassengerCompleted() {
        if (!isSeatsCompleted()) return false;
        try {
            var raw = sessionStorage.getItem('skybook-passenger-manifest');
            if (!raw) return false;
            var data = JSON.parse(raw);
            return !!(data && data.p1 && data.p1.firstName && data.p1.lastName);
        } catch (e) {
            return false;
        }
    }

    // Reset downstream data when entering an earlier step so forward steps are genuinely locked
    if (flowIndex === 0) {
        sessionStorage.removeItem('skybook-selected-flights');
        sessionStorage.removeItem('skybook-seat-assignments');
        sessionStorage.removeItem('skybook-passenger-manifest');
    } else if (flowIndex === 1) {
        sessionStorage.removeItem('skybook-passenger-manifest');
    }

    // Strict route gating: prevent direct URL navigation to later steps if prerequisites are missing
    if (flowIndex === 1) {
        if (!isFlightsCompleted()) {
            window.location.replace(routes.flights);
            return;
        }
    } else if (flowIndex === 2) {
        if (!isFlightsCompleted()) {
            window.location.replace(routes.flights);
            return;
        } else if (!isSeatsCompleted()) {
            window.location.replace(routes.seats);
            return;
        }
    } else if (flowIndex === 3) {
        if (!isFlightsCompleted()) {
            window.location.replace(routes.flights);
            return;
        } else if (!isSeatsCompleted()) {
            window.location.replace(routes.seats);
            return;
        } else if (!isPassengerCompleted()) {
            window.location.replace(routes.passenger);
            return;
        }
    }

    if (flowIndex >= 0) {
        var progress = document.createElement('nav');
        progress.id = 'skybook-booking-progress';
        progress.setAttribute('aria-label', 'Booking progress');

        // Gating policy:
        // - You can only navigate backwards to steps you have already passed.
        // - You CANNOT click forward to any step that hasn't been completed yet.
        var stepUnlocked = [
            true,                                              // Step 1 (Flights): always accessible
            flowIndex > 1 || (flowIndex === 0 && isFlightsCompleted()), // Step 2 (Seats): unlocked only if on later step or flights selected
            flowIndex > 2 || (flowIndex === 1 && isSeatsCompleted()),   // Step 3 (Passenger Details): unlocked only if on confirm or seats completed
            flowIndex === 3 || isPassengerCompleted()          // Step 4 (Confirm & Pay): unlocked only if passenger details completed
        ];

        progress.innerHTML = '<div class="skybook-progress-inner">' + flowSteps.map(function (step, index) {
            var separator = index ? '<span class="skybook-separator" aria-hidden="true">›</span>' : '';
            var isCurrent = index === flowIndex;
            var unlocked = stepUnlocked[index];
            var stepNum = index + 1;
            var stepId = 'skybook-step-btn-' + stepNum;

            if (isCurrent) {
                return separator +
                    '<span id="' + stepId + '" data-step-index="' + index + '" class="skybook-step skybook-current" aria-current="step">' +
                    '<span class="skybook-step-number">' + stepNum + '</span>' +
                    '<span>' + step[0].toUpperCase() + '</span></span>';
            } else if (unlocked) {
                return separator +
                    '<a id="' + stepId + '" data-step-index="' + index + '" class="skybook-step skybook-unlocked" href="' + step[1] + '" title="Go to ' + step[0] + '">' +
                    '<span class="skybook-step-number">' + stepNum + '</span>' +
                    '<span>' + step[0].toUpperCase() + '</span></a>';
            } else {
                return separator +
                    '<span id="' + stepId + '" data-step-index="' + index + '" class="skybook-step skybook-disabled" title="Locked - Complete previous step first" aria-disabled="true">' +
                    '<span class="skybook-step-number">' + stepNum + '</span>' +
                    '<span>' + step[0].toUpperCase() + '</span></span>';
            }
        }).join('') + '</div>';

        header.insertAdjacentElement('afterend', progress);

        window.SkyBookProgress = {
            unlockStep: function (stepNum) {
                var el = document.getElementById('skybook-step-btn-' + stepNum);
                if (el && el.classList.contains('skybook-disabled')) {
                    var idx = stepNum - 1;
                    var target = flowSteps[idx][1];
                    var label = flowSteps[idx][0].toUpperCase();
                    var newEl = document.createElement('a');
                    newEl.id = 'skybook-step-btn-' + stepNum;
                    newEl.setAttribute('data-step-index', idx);
                    newEl.className = 'skybook-step skybook-unlocked';
                    newEl.href = target;
                    newEl.title = 'Go to ' + flowSteps[idx][0];
                    newEl.innerHTML = '<span class="skybook-step-number">' + stepNum + '</span><span>' + label + '</span>';
                    el.parentNode.replaceChild(newEl, el);
                }
            },
            lockStep: function (stepNum) {
                var el = document.getElementById('skybook-step-btn-' + stepNum);
                if (el && !el.classList.contains('skybook-current') && !el.classList.contains('skybook-disabled')) {
                    var idx = stepNum - 1;
                    var label = flowSteps[idx][0].toUpperCase();
                    var newEl = document.createElement('span');
                    newEl.id = 'skybook-step-btn-' + stepNum;
                    newEl.setAttribute('data-step-index', idx);
                    newEl.className = 'skybook-step skybook-disabled';
                    newEl.title = 'Locked - Complete previous step first';
                    newEl.setAttribute('aria-disabled', 'true');
                    newEl.innerHTML = '<span class="skybook-step-number">' + stepNum + '</span><span>' + label + '</span>';
                    el.parentNode.replaceChild(newEl, el);
                }
            }
        };
    }
})();