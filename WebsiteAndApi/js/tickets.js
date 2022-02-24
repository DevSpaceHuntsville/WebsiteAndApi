function StudentTicketViewModel() {
	var Self = this;
	Self.Email = ko.observable();
	Self.StatusMessage = ko.observable();
	Self.isBusy = ko.observable(false);

	Self.GetCode = function () {
	    Self.isBusy(true);

		var CodeRequest = new XMLHttpRequest();
		CodeRequest.open('POST', `${currentEnvironment}/ticket`, true);
		CodeRequest.setRequestHeader('Content-Type', 'application/json');
		var str = '{"id":0,"email":"' + Self.Email() + '","code":null}';
        
		CodeRequest.send(str);

		CodeRequest.onreadystatechange = function () {
		    if (CodeRequest.readyState == CodeRequest.DONE) {
		        Self.isBusy(false);

				switch (CodeRequest.status) {
					case 201:
						Self.StatusMessage('You email address has been verified. The code has been sent.');
						break;

					case 204:
						Self.StatusMessage('A previous code was found. Original code was re-sent.');
						break;

					case 400:
						Self.StatusMessage('There was an error with the request.');
						break;

					case 500:
					case 502:
						Self.StatusMessage('There was a server side error. If this error persist, please contant info@devspaceconf.com');

					default:
						break;
				}
			}
		};
	};

	Self.showSpinnerWhenWaiting = function () {
	    if (Self.isBusy() === true) {
	        return '<img src="images/ajax-loader.gif" alt="loading, plese wait..." style="width: 10%; margin: 10px 40%;">';
	    } else {
	        return '<input type="submit" value="Get Student Access Code" />';
	    }
	}

	Self.showSpinner = ko.computed(Self.showSpinnerWhenWaiting, Self);
}

ko.applyBindings(new StudentTicketViewModel(), document.getElementById('StudentCodeForm'));
