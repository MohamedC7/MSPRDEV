import cv2
import logging
import sys

logging.basicConfig(filename='../example.log', level=logging.INFO)

def equal(image1, image2):
    original = cv2.imread(image1)
    duplicate = cv2.imread(image2)
    equals = False
    # 1) Check if 2 images are equals
    if original.shape == duplicate.shape:
        difference = cv2.subtract(original, duplicate)
        b, g, r = cv2.split(difference)
        if cv2.countNonZero(b) == 0 and cv2.countNonZero(g) == 0 and cv2.countNonZero(r) == 0:
            logging.info("The images are completely Equal")
        equals = True
    else:
        logging.info("the image are different")
    # cv2.imshow("Original", original)
    # cv2.imshow("Duplicate", duplicate)
    # cv2.waitKey(0)
    # cv2.destroyAllWindows()

    return equals



if __name__ == '__main__':
        print("script python is running")
        print(equal("model/2D/Ekans.png","model/2D/Ekans_Colored.png"))
    