import http from 'k6/http';
import { check } from 'k6';

export let options = {
    vus: 30,
    duration: '10s'
}

export default function () {
    const uniqueId = getUniqueId();
    const postUrl = "http://localhost:5001/api/url-shorts"
    
    const longUrl = `https://www.google.com/search?q=${uniqueId}`;
    const payload = JSON.stringify({
        url: longUrl,
        hasQrCode: true
    });
    const params = {
        headers: {
            'Content-Type': 'application/json'
        },
    }
    
    let res = http.post(postUrl, payload, params);

    check(res, {
        'status is 200': (r) => r.status === 200,
    });
}

function getUniqueId() {
    const ticks = new Date().getTime();
    let uniqueId = ticks.toString(16);
    const randomNum = Math.floor(Math.random() * 10000);
    uniqueId += randomNum;
    return uniqueId;
}