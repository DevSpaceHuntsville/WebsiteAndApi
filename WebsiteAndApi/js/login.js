function showMessage(msg) {
	const messageContainer = document.getElementById('error');
	messageContainer.innerText = msg;
	messageContainer.classList.remove('hidden');
	setTimeout(() => messageContainer.classList.add('hidden'), 10000);
}

function login(event) {
	event.preventDefault();
	document.body.style.cursor = 'wait';

	var Request = new XMLHttpRequest();
	Request.withCredentials = true;
	Request.open('GET', `${currentEnvironment}/Login`, true);
	Request.setRequestHeader('Accept', 'text/plain');
	const email = document.getElementById('accountEmail');
	const password = document.getElementById('password');
	Request.setRequestHeader('Authorization', 'Basic ' + btoa(email.value + ':' + password.value));
	Request.send();

	Request.onreadystatechange = function () {
		if (Request.readyState == Request.DONE) {
			switch (Request.status) {
				case 200:
					sessionStorage.setItem('Id', Request.responseText);
					window.location = '/profile.html';
					break;
				case 401:
					showMessage('Invalid Email or Password');
					break;
				default:
					showMessage('Something went wrong, please try again later');
					alert(Request.status);
					break;
			}
		}
	};
	document.body.style.cursor = '';
}

var Forced = false;
function Force() {
	if (Forced) { return; }
	document.body.style.cursor = 'wait';
	Forced = true;

	var Token = window.location.search.substring(7);

	var Request = new XMLHttpRequest();
	Request.withCredentials = true;
	Request.open('GET', `${currentEnvironment}/Login`, true);
	Request.setRequestHeader('Accept', 'text/plain');
	Request.setRequestHeader('Authorization', 'Force ' + Token);
	Request.send();

	Request.onreadystatechange = function () {
		if (Request.readyState == Request.DONE) {
			document.body.style.cursor = '';
			switch (Request.status) {
				case 200:
					sessionStorage.setItem('Id', Request.responseText);
					window.location = '/profile.html';
					break;
				default:
					showMessage("Invalid Token");
			}
			document.body.style.cursor = '';
		}
	};
}

function getToken() {
	document.body.style.cursor = 'wait';
	const email = document.getElementById('accountEmail').value;
	if (!email || !email.trim()) {
		alert('You must enter an email address.');
		document.body.style.cursor = '';
		return;
	}

	var Request = new XMLHttpRequest();
	Request.withCredentials = true;
	Request.open('GET', `${currentEnvironment}/Login/?Email=` + email, true);
	Request.setRequestHeader('Accept', 'text/plain');
	Request.send();

	Request.onreadystatechange = function () {
		if (Request.readyState !== Request.DONE) { return; }
		document.body.style.cursor = '';
		switch (Request.status) {
			case 200:
				const message = Request.responseText.replace('\"', '').replace('\"', '');
				showMessage(message);
				break;
			default:
				showMessage('Error Processing Request');
				break;
		}
	};
	document.body.style.cursor = '';
}

function register(event) {
	event.preventDefault();
	document.body.style.cursor = 'wait';

	const name = document.getElementById('displayName').value;
	const email = document.getElementById('accountEmail').value;
	const password = document.getElementById('password').value;

	if (!name || !name.trim()) {
		showMessage('Please provide a display name');
		document.body.style.cursor = '';
		return;
	}
	if (!email || !email.trim()) {
		showMessage('Please provide a valid email address');
		document.body.style.cursor = '';
		return;
	}
	if (!password || !password.trim()) {
		showMessage('Please provide a password');
		document.body.style.cursor = '';
		return;
	}

	var RequestJson = {
		DisplayName: name,
		EmailAddress: email,
		PasswordHash: password
	};

	var Request = new XMLHttpRequest();
	Request.withCredentials = true;
	Request.open('GET', `${currentEnvironment}/Login`, true);
	Request.setRequestHeader('Content-Type', 'application/json');
	Request.send(JSON.stringify(RequestJson));

	Request.onreadystatechange = function () {
		if (Request.readyState !== Request.DONE) { return; }
		switch (Request.status) {
			case 201:
				Login(event);
				break;
			case 401:
				showMessage('Could not create an account. An account with this email already exists');
				break;
			default:
				showMessage('An unknown error has occurred, please try again');
		}
		document.body.style.cursor = '';
	};

}

function showRegister() {
	const loginFields = Array.from(document.querySelectorAll('.login-only'));
	const registerFields = Array.from(document.querySelectorAll('.register-only'));
	const form = document.getElementById('authForm');
	const displayName = document.getElementById('displayName');


	loginFields.forEach(f => { f.classList.add('hidden'); });
	registerFields.forEach(f => f.classList.remove('hidden'));
	form.setAttribute('onsubmit', 'register(event)');
	displayName.setAttribute('required', '');
}

function showLogin() {
	const loginFields = Array.from(document.querySelectorAll('.login-only'));
	const registerFields = Array.from(document.querySelectorAll('.register-only'));
	const form = document.getElementById('authForm');
	const displayName = document.getElementById('displayName');

	loginFields.forEach(f => { f.classList.remove('hidden'); });
	registerFields.forEach(f => f.classList.add('hidden'));
	form.setAttribute('onsubmit', 'login(event)');
	displayName.removeAttribute('required');
}

function togglePassword() {
	const icons = Array.from(document.getElementById('passwordToggle').children);
	icons.forEach(i => i.classList.toggle('hidden'));
	const passwordInput = document.getElementById('password');
	if (passwordInput.type === 'password') {
		passwordInput.setAttribute('type', 'text');
	} else {
		passwordInput.setAttribute('type', 'password');
	}
}
