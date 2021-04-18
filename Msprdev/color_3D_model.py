import matplotlib.pyplot as plt
import numpy as np
import cv2


class Color3D:

    def setColor(img,color):
        nemo = cv2.imread('./model3D/nemo.jpg')
        plt.imshow(nemo)
        plt.show()

        nemo = cv2.cvtColor(nemo, cv2.COLOR_BGR2RGB)
        plt.imshow(nemo)
        plt.show()
        return nemo

