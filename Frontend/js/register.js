document
.getElementById("registerForm")
.addEventListener("submit", async function(e){

e.preventDefault()

const user = {

firstName: document.getElementById("firstName").value,
lastName: document.getElementById("lastName").value,
email: document.getElementById("email").value,
passwordHash: document.getElementById("password").value

}

const response = await fetch("http://localhost:5149/api/auth/register",{

method:"POST",
headers:{
"Content-Type":"application/json"
},
body: JSON.stringify(user)

})

if(response.ok){

alert("Registration successful")

window.location.href="login.html"

}else{

alert("Register failed")

}

})