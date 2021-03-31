function Article(data) {
	var Self = this;
	Self.Title = ko.observable();
	Self.Body = ko.observable();
	Self.PublishDate = ko.observable();
	Self.ExpireDate = ko.observable();

	if (data) {
		Self.Title(data.Title);
		Self.Body(data.Body);
		Self.PublishDate(data.PublishDate);
		Self.ExpireDate(data.ExpireDate);
	}
}

function ContentViewModel() {
	var Self = this;
	Self.Content = ko.observableArray([]);

	var qd = null;
	if (location.search) {
		qd = {};
		location.search.substr(1).split("&").forEach(function (item) { var s = item.split("="), k = s[0], v = s[1] && decodeURIComponent(s[1]); (k in qd) ? qd[k].push(v) : qd[k] = [v] });
	}

	var ContentRequest = new XMLHttpRequest();
	if (qd && qd.id)
		ContentRequest.open('GET', '/api/v1/content/' + qd.id, true);
	else
		ContentRequest.open('GET', '/api/v1/content', true);
	ContentRequest.send();

	ContentRequest.onreadystatechange = function () {
		if (ContentRequest.readyState == ContentRequest.DONE) {
			switch (ContentRequest.status) {
				case 200:
					var ArticleList = JSON.parse(ContentRequest.responseText);
					if (ArticleList.length)
						for (var index = 0; index < ArticleList.length; ++index)
							Self.Content.push(new Article(ArticleList[index]));
					else
						Self.Content.push(new Article(ArticleList));
					break;

				case 500:
				default:
					break;
			}
		}
	};

//	Self.GetCode = function () {
//	    Self.isBusy(true);

//		var CodeRequest = new XMLHttpRequest();
//		CodeRequest.open('POST', '/api/v1/ticket', true);
//		CodeRequest.setRequestHeader('Content-Type', 'application/json');
//		var str = '{"id":0,"email":"' + Self.Email() + '","code":null}';
        
//		CodeRequest.send(str);

//		CodeRequest.onreadystatechange = function () {
//		    if (CodeRequest.readyState == CodeRequest.DONE) {
//		        Self.isBusy(false);

//				switch (CodeRequest.status) {
//					case 201:
//						Self.StatusMessage('You email address has been verified. The code has been sent.');
//						break;

//					case 204:
//						Self.StatusMessage('A previous code was found. Original code was re-sent.');
//						break;

//					case 400:
//						Self.StatusMessage('There was an error with the request.');
//						break;

//					case 500:
//					case 502:
//						Self.StatusMessage('There was a server side error. If this error persist, please contant info@devspaceconf.com');

//					default:
//						break;
//				}
//			}
//		};
//	};
}

ko.applyBindings(new ContentViewModel(), document.getElementById('Content'));
