from googletrans import Translator


def translate(text):
    try:
        trandlate_text = Translator().translate(text, src="en", dest="zh-TW")
        return trandlate_text.text
    except:
        return "翻譯錯誤"


""" while True:
    try:
        original_text = input()
        trandlate_text = Translator().translate(original_text, src="en", dest="zh-TW")
        print(trandlate_text.text)
    except KeyboardInterrupt:
        break
    except:
        print("翻譯錯誤")
 """
