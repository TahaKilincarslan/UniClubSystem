document
.getElementById("loginForm")
.addEventListener("submit", async function(e){

e.preventDefault()

const loginData = {

email: document.getElementById("email").value,
password: document.getElementById("password").value

}

const response = await fetch("http://localhost:5149/api/auth/login",{

method:"POST",
headers:{
"Content-Type":"application/json"
},
body: JSON.stringify(loginData)

})

if(response.ok){

const user = await response.json()

// kullanıcıyı tarayıcıya kaydet
localStorage.setItem("user", JSON.stringify(user))

// anasayfaya yönlendir
window.location.href="index.html"

}else{

alert("Invalid email or password")

}

})