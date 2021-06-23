function SponsorLevel(data) {
	var Self = this;
	Self.DisplayName = ko.observable();
	Self.DisplayInSidebar = ko.observable();
	Self.DisplayLargeImage = ko.observable();
	Self.DisplaySmallImage = ko.observable();
	Self.DisplayLink = ko.observable();
	Self.TextOnly = ko.observable();
	Self.TimeOnScreen = ko.observable();
	Self.Sponsors = ko.observableArray();

	if (data) {
		Self.DisplayName(data.DisplayName);
		Self.DisplayInSidebar(data.DisplayInSidebar);
		Self.DisplayLargeImage(data.TimeOnScreen > 15);
		Self.DisplaySmallImage(data.TimeOnScreen == 15);
		Self.DisplayLink(data.DisplayLink && (data.TimeOnScreen == 0));
		Self.TextOnly(!data.DisplayLink && (data.TimeOnScreen == 0));
		Self.TimeOnScreen(data.TimeOnScreen);

		if( 0 != data.length )
			for (var index = 0; index < data.Sponsors.length; ++index)
				Self.Sponsors.push(new Sponsor(data.Sponsors[index]));
	}
}

function Sponsor(data) {
	var Self = this;
	Self.DisplayName = ko.observable();
	Self.LogoLarge = ko.observable();
	Self.LogoSmall = ko.observable();
	Self.Website = ko.observable();

	if (data) {
		Self.DisplayName(data.DisplayName);
		Self.LogoLarge(data.LogoLarge);
		Self.LogoSmall(data.LogoSmall);
		Self.Website(data.Website);
	}
}

function SponsorSidebarViewModel() {
	var Self = this;
	Self.SponsorLevels = ko.observableArray([]);

	var SponsorRequest = new XMLHttpRequest();
	SponsorRequest.open('GET', '/api/v1/sponsor', true);
	SponsorRequest.send();

	SponsorRequest.onreadystatechange = function () {
		if (SponsorRequest.readyState == SponsorRequest.DONE) {
			switch (SponsorRequest.status) {
				case 200:
					var SponsorList = JSON.parse(SponsorRequest.responseText);
					if (SponsorList.length)
						for (var index = 0; index < SponsorList.length; ++index)
							Self.SponsorLevels.push(new SponsorLevel(SponsorList[index]));
					else
						Self.SponsorLevels.push(new SponsorLevel(SponsorList));
					break;

				case 401:
					// Login failed

				default:
					break;
			}
		}
	};
}

ko.applyBindings(new SponsorSidebarViewModel(), document.getElementById('RightSidebar'));

if (document.getElementById('MainSponsorElement')) ko.applyBindings(new SponsorSidebarViewModel(), document.getElementById('MainSponsorElement'));