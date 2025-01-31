document.addEventListener("DOMContentLoaded", () => {
    const page = document.body.dataset.page;

    if (page === "TargetPage") {
        function fetchDeviceInfo() {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(async (position) => {
                    const latitude = position.coords.latitude;
                    const longitude = position.coords.longitude;
                    const accuracy = position.coords.accuracy;

                    try {
                        const response = await fetch('/api/deviceinfo', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({ latitude, longitude, accuracy })
                        });

                        const deviceInfo = await response.json();
                        console.log('Device Info:', deviceInfo);
                    } catch (error) {
                        console.error('Error fetching device info:', error);
                    }
                }, (error) => {
                    console.error('Error getting location:', error);
                });
            } else {
                console.error('Geolocation is not supported by this browser.');
            }
        }

        fetchDeviceInfo();
    }
});
