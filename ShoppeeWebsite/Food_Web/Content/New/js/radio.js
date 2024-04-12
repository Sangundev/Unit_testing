var images = [];

images[0] = ['banner1.jpg'];
images[1] = ['banner2.jpg'];
images[2] = ['banner2-cut.png'];
var index = 0;

function change() {
  document.getElementById("mainPhoto").src = images[index];
  if (index == 2) {
    index = 0;
  } else {
    index++;
  }

  setTimeout(change, 5000);
}

window.onload = change();
